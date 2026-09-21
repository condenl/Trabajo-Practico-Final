$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if ($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')) { throw 'Proyecto incorrecto' }
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$specs=Get-Content (Join-Path $out 'sequences-reconciled.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$specs+=Get-Content (Join-Path $out 'model-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
# Frame bounds use EA's sequence spacing (35 units per message).
$frames=@{
 'N01-06a'=@(@(2,4,4,'Ofertas insuficientes'),@(0,8,10,'Rechazada|Aprobada',55));
 'N01-06b'=,@(0,5,11,'No conforme|Conforme',90);
 'N02-06a'=@(@(2,2,2,'No acepta membresia'),@(2,8,9,'Datos invalidos'));
 'N02-06b'=@(@(2,4,4,'Sin stock'),@(1,7,8,'Cliente identificado'));
 'N02-06c'=,@(0,4,7,'Autoriza marketing|No autoriza',125);
 'N03-06'=@(@(2,8,8,'Informacion incompleta'),@(2,12,12,'No conveniente'))
}
foreach($code in $frames.Keys) {
    $m=$manifest | Where-Object code -eq $code
    if (@($m).Count -ne 1) { throw ('Manifest ambiguo: '+$code) }
    $d=$repo.GetDiagramByID($m.diagramId); $p=$repo.GetPackageByID($d.PackageID)
    $p.Elements.Refresh()
    for($i=$p.Elements.Count-1;$i -ge 0;$i--) { if($p.Elements.GetAt($i).Type -eq 'InteractionFragment') { $p.Elements.DeleteAt($i,$false) } }
    $p.Elements.Refresh();$d.DiagramObjects.Refresh()
    $s=$specs | Where-Object code -eq $code
    foreach($o in $d.DiagramObjects) {
        $e=$repo.GetElementByID($o.ElementID)
        if($e.Type -eq 'Object') {
            $n=$s.nodes | Where-Object name -eq $e.Name
            $o.left=$n.x+180; $o.right=$n.x+$n.w+180; $o.top=-50; $o.bottom=-140
            [void]$o.Update()
        }
    }
    $roles=@($p.Elements | Where-Object Type -eq 'Object').Count
    foreach($f in $frames[$code]) {
        $e=$p.Elements.AddNew('','InteractionFragment'); $e.Subtype=$f[0]; [void]$e.Update()
        $parts=$f[3] -split '\|'
        foreach($name in $parts) { $part=$e.Partitions.AddNew($name,'Partition'); $part.Size=if($parts.Count -gt 1){$f[4]}else{50} }
        [void]$e.Update()
        $top=135+35*($f[1]-1)-30; $bottom=135+35*($f[2]-1)+20
        $obj=$d.DiagramObjects.AddNew(('l=10;r={0};t={1};b={2};' -f (280+$roles*290),$top,$bottom),'')
        $obj.ElementID=$e.ElementID; $obj.Sequence=100; [void]$obj.Update()
    }
    [void]$d.Update();$repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
    Write-Output ('FRAMED '+$code)
}
