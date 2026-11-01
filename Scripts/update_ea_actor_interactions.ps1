param([switch]$SpecsOnly)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$backup=Join-Path $root ('Backup_EA_Actors_'+(Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Path $backup|Out-Null
Copy-Item (Join-Path $root 'diagramas.EAP') (Join-Path $backup 'diagramas.EAP')
$specs=Get-Content (Join-Path $out 'code-design-routed.json') -Raw -Encoding UTF8|ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8|ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8|ConvertFrom-Json
foreach($s in @($specs|Where-Object {$_.kind -eq 'Sequence' -and -not $SpecsOnly})){
    $m=@($manifest|Where-Object code -eq $s.code)[0];$d=$repo.GetDiagramByGuid($m.guid)
    $repo.CloseDiagram($d.DiagramID)
    $elements=@{};$objects=@{}
    foreach($o in $d.DiagramObjects){
        $e=$repo.GetElementByID($o.ElementID)
        if($e.Type -eq 'Object'){$key=$repo.GetElementByID($e.ClassifierID).Name}else{$key=$e.Name}
        $elements[$key]=$e;$objects[$key]=$o
    }
    $old=@($d.DiagramLinks|ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)}|Where-Object Type -eq 'Sequence'|Sort-Object SequenceNo)
    $internal=@($old|Where-Object {$repo.GetElementByID($_.ClientID).Type -ne 'Actor' -and $repo.GetElementByID($_.SupplierID).Type -ne 'Actor'})
    if($internal.Count -ne @($s.links|Where-Object {-not $_.actorInteraction}).Count){throw 'Internal messages changed'}
    foreach($n in $s.nodes){
        if(-not $elements.ContainsKey($n.key)){
            if($n.kind -ne 'Actor'){throw 'Missing existing lifeline'}
            $e=$repo.GetPackageByID($d.PackageID).Elements.AddNew($n.name,'Actor');[void]$e.Update()
            $elements[$n.key]=$e
            $o=$d.DiagramObjects.AddNew('','');$o.ElementID=$e.ElementID;$objects[$n.key]=$o
        }
        $o=$objects[$n.key];$o.left=$n.x;$o.right=$n.x+$n.w;$o.top=-$n.y;$o.bottom=-($n.y+$n.h);[void]$o.Update()
    }
    $i=0;$used=@{}
    foreach($l in $s.links){
        if($l.actorInteraction){
            $matches=@($old|Where-Object {-not $used.ContainsKey($_.ConnectorID) -and $_.Name -eq $l.name -and $_.ClientID -eq $elements[$l.source].ElementID -and $_.SupplierID -eq $elements[$l.target].ElementID})
            if($matches.Count){$c=$matches[0]}else{
                $c=$elements[$l.source].Connectors.AddNew($l.name,'Sequence');$c.SupplierID=$elements[$l.target].ElementID;$c.DiagramID=$d.DiagramID
                [void]$c.Update();$dl=$d.DiagramLinks.AddNew('','');$dl.ConnectorID=$c.ConnectorID;[void]$dl.Update()
            }
        }else{
            $c=$internal[$i++]
            if($c.Name -cne $l.name -or $c.ClientID -ne $elements[$l.source].ElementID -or $c.SupplierID -ne $elements[$l.target].ElementID){throw 'Internal message mismatch'}
        }
        $used[$c.ConnectorID]=$true
        $c.SequenceNo=$l.sequence;$c.TransitionAction='Call';$c.TransitionEvent='Synchronous';[void]$c.Update()
        $c.Properties.Item('IsReturn').Value=[bool]$l.isReturn;[void]$c.Update()
    }
    $frames=@($d.DiagramObjects|Where-Object {$repo.GetElementByID($_.ElementID).Type -eq 'InteractionFragment'})
    for($i=0;$i -lt $frames.Count;$i++){
        $f=$s.frames[$i];$o=$frames[$i];$e=$repo.GetElementByID($o.ElementID)
        $o.left=10;$o.right=[int](($s.nodes|Measure-Object x -Maximum).Maximum+300)
        $o.top=-(135+35*($f.first-1)-20);$o.bottom=-(135+35*($f.last-1)+15);[void]$o.Update()
        foreach($part in $e.Partitions){$part.Size=$f.size};[void]$e.Update()
    }
    $d.Notes=$s.notes;[void]$d.Update();$repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
    Write-Output ('UPDATED '+$s.code)
}
$text=Get-Content (Join-Path $root 'Documentacion\casos de uso\Casos_de_Uso_N01_N02.md') -Raw -Encoding UTF8
foreach($match in [regex]::Matches($text,'## (CU-N\d+-\d+)[^\n]*\n(.*?)(?=\n## |\z)','Singleline')){
    $code=$match.Groups[1].Value;$notes=$match.Groups[2].Value.Replace('<br>',"`n")
    $sql="SELECT Name,ea_guid FROM t_object WHERE Object_Type='UseCase'"
    [xml]$result=$repo.SQLQuery($sql)
    $rows=@($result.SelectNodes('//Row')|Where-Object {$_.Name.StartsWith($code)})
    if(-not $rows.Count){throw ('Use case not found: '+$code)}
    foreach($row in $rows){$e=$repo.GetElementByGuid($row.ea_guid);$e.Notes=$notes;[void]$e.Update();Write-Output ('SPEC '+$e.Name)}
}
