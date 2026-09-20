$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if ($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')) { throw 'Proyecto incorrecto' }
$manifest=Get-Content (Join-Path $out 'manifest-n03.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$specs=Get-Content (Join-Path $out 'model-n03.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($m in $manifest) {
    $d=$repo.GetDiagramByID($m.diagramId)
    if ($m.code -eq 'N03-02') {
        $spec=$specs | Where-Object code -eq $m.code
        foreach ($obj in $d.DiagramObjects) {
            $e=$repo.GetElementByID($obj.ElementID)
            $n=@($spec.nodes | Where-Object name -eq $e.Name) | Select-Object -First 1
            if ($e.Type -eq 'StateNode') {
                # Final nodes have no names; the rejected final is in the middle row.
                if ($obj.top -lt -1000 -and $obj.top -gt -1200) { $obj.left=350; $obj.right=385; [void]$obj.Update() }
                continue
            }
            if ($n.key -eq 'lane0') { $obj.right=540 }
            elseif ($n.key -eq 'reject') { $obj.left=300; $obj.right=500 }
            elseif ($n.x -gt 280) { $obj.left=$n.x+270; $obj.right=$n.x+$n.w+270 }
            [void]$obj.Update()
        }
    }
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
}
