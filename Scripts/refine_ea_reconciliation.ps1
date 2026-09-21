$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if ($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')) { throw 'Proyecto incorrecto' }
$manifest=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$models=Get-Content (Join-Path $out 'global-routes.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach($s in $models) {
    $m=$manifest | Where-Object code -eq $s.code; $d=$repo.GetDiagramByID($m.diagramId)
    foreach($o in $d.DiagramObjects) {
        $e=$repo.GetElementByID($o.ElementID)
        $n=@($s.nodes | Where-Object name -eq $e.Name) | Select-Object -First 1
        if($n) { $o.left=$n.x; $o.right=$n.x+$n.w; $o.top=-$n.y; $o.bottom=-$n.y-$n.h; [void]$o.Update() }
    }
    foreach($dl in $d.DiagramLinks) {
        $c=$repo.GetConnectorByID($dl.ConnectorID)
        $a=$repo.GetElementByID($c.ClientID).Name; $b=$repo.GetElementByID($c.SupplierID).Name
        $l=@($s.links | Where-Object { $_.source -eq $a -and $_.target -eq $b -and $_.name -eq $c.Name }) | Select-Object -First 1
        if($l) { $dl.Style='Mode=3;Color=-1;LWidth=0;'; $dl.Path=$l.path; [void]$dl.Update() }
    }
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
}
