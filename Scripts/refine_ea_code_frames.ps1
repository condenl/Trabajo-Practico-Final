$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$specs=Get-Content (Join-Path $out 'code-design-routed.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach($s in $specs | Where-Object frames){
    $m=@($manifest | Where-Object code -eq $s.code)[0]
    $d=$repo.GetDiagramByID($m.diagramId)
    $fragments=@($d.DiagramObjects | Where-Object { $repo.GetElementByID($_.ElementID).Type -eq 'InteractionFragment' })
    if($fragments.Count -ne $s.frames.Count){throw ('Cantidad de fragmentos inesperada '+$s.code)}
    $repo.CloseDiagram($d.DiagramID)
    for($i=0;$i -lt $fragments.Count;$i++){
        $f=$s.frames[$i];$o=$fragments[$i]
        $lastCall=@($s.links | Where-Object sequence -eq $f.last)[0]
        $tail=if($lastCall.source -eq $lastCall.target){26}else{8}
        $o.top=-(135+35*($f.first-1)-25)
        $o.bottom=-(135+35*($f.last-1)+$tail)
        [void]$o.Update()
    }
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
    Write-Output ('REFINED '+$s.code)
}
