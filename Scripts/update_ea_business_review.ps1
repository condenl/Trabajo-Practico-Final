param([switch]$VerifyOnly,[string]$Codes='')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$manifest=@()
foreach($name in @('manifest.json','manifest-n03.json','manifest-reconciliation.json')){
    $manifest+=Get-Content (Join-Path $out $name) -Raw -Encoding UTF8|ConvertFrom-Json
}
$specs=Get-Content (Join-Path $out 'business-review.json') -Raw -Encoding UTF8|ConvertFrom-Json
if(-not $VerifyOnly){
    $backup=Join-Path $root ('Backup_EA_BusinessReview_'+(Get-Date -Format 'yyyyMMdd_HHmmss'))
    New-Item -ItemType Directory -Path $backup|Out-Null
    Copy-Item -LiteralPath (Join-Path $root 'diagramas.EAP') -Destination (Join-Path $backup 'diagramas.EAP')
}
$report=@()
foreach($spec in $specs){
    if($Codes -and $spec.code -notin ($Codes -split ',')){continue}
    $entry=@($manifest|Where-Object code -eq $spec.code)
    if($entry.Count -ne 1){throw ('Manifest ambiguo: '+$spec.code)}
    $d=$repo.GetDiagramByGuid($entry[0].guid);$p=$repo.GetPackageByID($d.PackageID)
    if($p.Diagrams.Count -ne 1 -or $p.Name -notlike ($spec.code+' *')){throw 'Paquete fuera de alcance'}
    if(-not $VerifyOnly){
        $repo.CloseDiagram($d.DiagramID)
        $arch=$p.Packages.AddNew('Version anterior '+(Get-Date -Format 'yyyyMMdd_HHmmss'),'');[void]$arch.Update()
        foreach($o in @($d.DiagramObjects)){
            $e=$repo.GetElementByID($o.ElementID)
            if($e.PackageID -ne $p.PackageID){throw 'Elemento compartido fuera del paquete'}
            $e.PackageID=$arch.PackageID;[void]$e.Update()
        }
        for($i=$d.DiagramObjects.Count-1;$i -ge 0;$i--){$d.DiagramObjects.DeleteAt($i,$false)}
        for($i=$d.DiagramLinks.Count-1;$i -ge 0;$i--){$d.DiagramLinks.DeleteAt($i,$false)}
        $d.DiagramObjects.Refresh();$d.DiagramLinks.Refresh();$p.Elements.Refresh()
        $d.Notes=$spec.notes;$d.ShowDetails=0;[void]$d.Update()
        $elements=@{}
        foreach($n in $spec.nodes){
            $e=$p.Elements.AddNew($n.name,$n.kind)
            if($n.notes){$e.Notes=$n.notes}
            if($n.subtype){$e.Subtype=$n.subtype}
            [void]$e.Update();$elements[$n.key]=$e
            $o=$d.DiagramObjects.AddNew(('l={0};r={1};t={2};b={3};' -f $n.x,($n.x+$n.w),$n.y,($n.y+$n.h)),'')
            $o.ElementID=$e.ElementID
            if($n.kind -in @('ActivityPartition','Boundary')){$o.Sequence=100}
            if($n.fontSize){$o.Style=('font=Arial;fontsz={0};bold=0;italic=0;ul=0;charset=0;pitch=34;' -f ($n.fontSize*10))}
            [void]$o.Update()
        }
        foreach($l in $spec.links){
            $c=$elements[$l.source].Connectors.AddNew($l.name,$l.kind);$c.SupplierID=$elements[$l.target].ElementID
            if($l.stereotype){$c.Stereotype=$l.stereotype}
            [void]$c.Update()
            $dl=$d.DiagramLinks.AddNew('','');$dl.ConnectorID=$c.ConnectorID
            if($l.path){$dl.Style='Mode=3;Color=-1;LWidth=0;';$dl.Path=$l.path}
            elseif($l.kind -in @('ControlFlow','ObjectFlow')){$dl.Style='Mode=3;Color=-1;LWidth=0;'}
            [void]$dl.Update()
        }
        [void]$d.Update();$repo.ReloadDiagram($d.DiagramID)
        if(-not $repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$entry[0].file,1)){throw 'Error al exportar'}
    }
    $d=$repo.GetDiagramByGuid($entry[0].guid)
    $d.DiagramObjects.Refresh();$d.DiagramLinks.Refresh()
    $objects=@($d.DiagramObjects)
    $elements=@($objects|ForEach-Object {$repo.GetElementByID($_.ElementID)})
    $links=@($d.DiagramLinks|Where-Object {-not $_.IsHidden}|ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)})
    if($elements.Count -ne $spec.nodes.Count -or $links.Count -ne $spec.links.Count){throw ('Conteo incorrecto: '+$spec.code+' '+$elements.Count+'/'+$spec.nodes.Count+' '+$links.Count+'/'+$spec.links.Count)}
    foreach($n in $spec.nodes){
        $found=@($elements|Where-Object {$_.Name -ceq $n.name -and $_.Type -eq $n.kind})
        if(-not $found.Count){throw ('Falta elemento '+$n.key)}
    }
    $report+=@{code=$spec.code;guid=$d.DiagramGUID;elements=$elements.Count;connectors=$links.Count;objectFlows=@($links|Where-Object Type -eq 'ObjectFlow').Count}
    Write-Output ('VERIFIED '+$spec.code)
}
$entry=@($manifest|Where-Object code -eq 'N03-06')[0]
$d=$repo.GetDiagramByGuid($entry.guid)
$messages=@($d.DiagramLinks|ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)}|Where-Object Type -eq 'Sequence'|Sort-Object SequenceNo)
if($messages.Count -ne 52 -or $repo.GetElementByID($messages[0].ClientID).Type -ne 'Actor'){throw 'Secuencia N03 inesperada'}
if(-not $VerifyOnly){
    $repo.CloseDiagram($d.DiagramID)
    # Native EA "Extend Source Activation Down": keep the actor participating
    # throughout the three requests. Each software invocation retains its reply.
    foreach($c in $messages|Where-Object {$repo.GetElementByID($_.ClientID).Type -eq 'Actor' -and -not $_.Properties.Item('IsReturn').Value}){
        $c.StateFlags=($c.StateFlags -replace 'ForceActivation=[^;]*;','')+'ForceActivation=1;'
        [void]$c.Update()
    }
    $repo.ReloadDiagram($d.DiagramID)
    if(-not $repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$entry.file,1)){throw 'Error al exportar secuencia'}
}
if($repo.GetConnectorByID($messages[0].ConnectorID).StateFlags -notmatch 'ForceActivation=1;'){throw 'Falta activación continua del actor'}
$report+=@{code='N03-06';guid=$d.DiagramGUID;messages=$messages.Count;returns=@($messages|Where-Object {$_.Properties.Item('IsReturn').Value -eq $true}).Count;actorActivation='Extend Source Activation Down'}
$report|ConvertTo-Json -Depth 10|Set-Content (Join-Path $out 'verification-business-review.json') -Encoding UTF8
$repo.RefreshModelView(0)
