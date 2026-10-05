param([string]$Codes='')
# Preserve current lifelines, classifiers, diagram IDs and user changes elsewhere.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$specs=Get-Content (Join-Path $out 'code-design-routed.json') -Raw -Encoding UTF8|ConvertFrom-Json
$specs=@($specs|Where-Object {$_.kind -eq 'Sequence' -and (-not $Codes -or $_.code -in ($Codes -split ','))})
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8|ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8|ConvertFrom-Json
$plans=@()
foreach($s in $specs){
    $m=@($manifest|Where-Object code -eq $s.code)[0]
    $d=$repo.GetDiagramByGuid($m.guid)
    $all=@($d.DiagramLinks|ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)}|Where-Object Type -eq 'Sequence'|Sort-Object SequenceNo)
    $calls=@($all|Where-Object {-not $_.Properties.Item('IsReturn').Value})
    $expected=@($s.links|Where-Object {-not $_.isReturn})
    if($calls.Count -ne $expected.Count){throw ('Llamadas modificadas: '+$s.code)}
    $lifelines=@{}
    foreach($o in $d.DiagramObjects){
        $e=$repo.GetElementByID($o.ElementID)
        if($e.Type -eq 'Object'){$lifelines[$repo.GetElementByID($e.ClassifierID).Name]=$e}
    }
    for($i=0;$i -lt $calls.Count;$i++){
        $c=$calls[$i];$l=$expected[$i]
        if($c.Name -cne $l.name -or $c.ClientID -ne $lifelines[$l.source].ElementID -or $c.SupplierID -ne $lifelines[$l.target].ElementID){throw ('Llamada diferente: '+$s.code+' '+$l.name)}
    }
    $replies=@($all|Where-Object {$_.Properties.Item('IsReturn').Value})
    if($replies.Count -notin @(0,$calls.Count)){throw ('Respuestas preexistentes no reconocidas: '+$s.code)}
    if($replies.Count){
        $expectedReplies=@($s.links|Where-Object isReturn)
        for($i=0;$i -lt $replies.Count;$i++){
            $c=$replies[$i];$l=$expectedReplies[$i]
            if($c.Name -cne $l.name -or $c.ClientID -ne $lifelines[$l.source].ElementID -or $c.SupplierID -ne $lifelines[$l.target].ElementID){throw ('Respuesta diferente: '+$s.code)}
        }
    }
    $frames=@($d.DiagramObjects|Where-Object {$repo.GetElementByID($_.ElementID).Type -eq 'InteractionFragment'})
    if($frames.Count -ne $s.frames.Count){throw ('Fragmentos modificados: '+$s.code)}
    $plans+=@{spec=$s;manifest=$m;diagram=$d;calls=$calls;replies=$replies;lifelines=$lifelines;frames=$frames}
}
$backup=Join-Path $root ('Backup_EA_Returns_'+(Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Path $backup|Out-Null
Copy-Item -LiteralPath (Join-Path $root 'diagramas.EAP') -Destination (Join-Path $backup 'diagramas.EAP')
Write-Output ('BACKUP '+$backup)
foreach($plan in $plans){
    $s=$plan.spec;$d=$plan.diagram;$lifelines=$plan.lifelines
    $repo.CloseDiagram($d.DiagramID)
    $ci=0;$ri=0
    foreach($l in $s.links){
        if(-not $l.isReturn){$c=$plan.calls[$ci++]}elseif($plan.replies.Count){$c=$plan.replies[$ri++]}else{
            $c=$lifelines[$l.source].Connectors.AddNew($l.name,'Sequence')
            $c.SupplierID=$lifelines[$l.target].ElementID
            $c.DiagramID=$d.DiagramID
        }
        $c.SequenceNo=$l.sequence
        $c.TransitionAction='Call';$c.TransitionEvent='Synchronous'
        [void]$c.Update()
        $c.Properties.Item('IsReturn').Value=[bool]$l.isReturn
        [void]$c.Update()
        if([bool]$c.Properties.Item('IsReturn').Value -ne [bool]$l.isReturn){throw 'Native IsReturn flag failed'}
        if($l.isReturn -and -not $plan.replies.Count){
            $dl=$d.DiagramLinks.AddNew('','');$dl.ConnectorID=$c.ConnectorID;[void]$dl.Update()
        }
    }
    for($i=0;$i -lt $plan.frames.Count;$i++){
        $f=$s.frames[$i];$o=$plan.frames[$i];$e=$repo.GetElementByID($o.ElementID)
        $last=@($s.links|Where-Object sequence -eq $f.last)[0]
        $tail=if($last.source -eq $last.target){26}else{8}
        $previous=@($s.links|Where-Object sequence -eq ($f.first-1))
        $head=if($previous.Count -and $previous[0].source -eq $previous[0].target){15}else{25}
        $o.top=-(135+35*($f.first-1)-$head);$o.bottom=-(135+35*($f.last-1)+$tail)
        [void]$o.Update()
        foreach($partition in $e.Partitions){$partition.Size=$f.size}
        [void]$e.Update()
    }
    $d.Notes=$s.notes
    $d.StyleEx=($d.StyleEx -replace 'SuppressBrackets=[^;]*;','')+'SuppressBrackets=1;'
    [void]$d.Update()
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$plan.manifest.file,1)
    Write-Output ('UPDATED '+$s.code+' '+$s.links.Count+' messages')
}
