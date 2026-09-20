$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo = ([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if ($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')) { throw 'Proyecto incorrecto' }
$manifest = Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
function Move-Element($diagram, $name, $x, $y) {
    foreach ($obj in $diagram.DiagramObjects) {
        if ($repo.GetElementByID($obj.ElementID).Name -eq $name) {
            $width=$obj.right-$obj.left; $height=$obj.top-$obj.bottom
            $obj.left=$x; $obj.right=$x+$width; $obj.top=-$y; $obj.bottom=-$y-$height
            [void]$obj.Update()
        }
    }
}
foreach ($m in $manifest) {
    $d=$repo.GetDiagramByID($m.diagramId)
    if ($m.code -eq 'N01-02') {
        Move-Element $d 'Informar motivo de rechazo' 582 835
        Move-Element $d 'Reclamar diferencias; no ingresar lote inválido' 42 1335
        # Move final nodes according to their predecessor so ownership is consistent.
        foreach ($obj in $d.DiagramObjects) {
            $e=$repo.GetElementByID($obj.ElementID)
            if ($e.Type -eq 'StateNode' -and $obj.left -gt 300 -and $obj.left -lt 550) {
                if ($obj.top -gt -1100) { $obj.left=672; $obj.right=707 }
                else { $obj.left=132; $obj.right=167 }
                [void]$obj.Update()
            }
        }
    }
    if ($m.code -eq 'N02-02') {
        foreach ($obj in $d.DiagramObjects) {
            $e=$repo.GetElementByID($obj.ElementID)
            if ($e.Name -eq 'Informar faltante y no concretar venta') {
                $e.Name='Conocer el faltante y desistir de la compra'; [void]$e.Update()
            }
        }
    }
    if ($m.code -eq 'N01-03') { Move-Element $d 'Detalle de compra' 350 480 }
    if ($m.code -in @('N02-03','N02-04','N02-07')) {
        $scale=1
        if ($m.code -eq 'N02-03') { $scale=0.75 }
        $positions=@{
            'Cliente'=@(40,40); 'Membresia'=@(40,420); 'Membresía'=@(40,420);
            'Venta'=@(465,40); 'Usuarios'=@(890,40);
            'DetalleVenta'=@(465,460); 'Detalle de venta'=@(465,460);
            'Producto'=@(40,840); 'Lote'=@(890,840)
        }
        foreach ($name in $positions.Keys) { Move-Element $d $name ([int]($positions[$name][0]*$scale)) ([int]($positions[$name][1]*$scale)) }
    }
    if ($d.Type -eq 'Logical') {
        $mode=2
        if ($m.code -in @('N01-03','N01-05','N02-05')) { $mode=1 }
        foreach ($dl in $d.DiagramLinks) { $dl.Style="Mode=$mode;Color=-1;LWidth=0;"; [void]$dl.Update() }
    }
    if ($m.code -in @('N01-05','N02-05')) {
        foreach ($obj in $d.DiagramObjects) {
            $e=$repo.GetElementByID($obj.ElementID)
            if ($e.Stereotype -eq 'BLL') {
                $src=Get-Content (Join-Path $root ('BLL\'+$e.Name+'.cs')) -Raw -Encoding UTF8
                foreach ($method in $e.Methods) {
                    $match=[regex]::Match($src, ('public\s+[\w<>?,.]+\s+'+$method.Name+'\s*\(([^)]*)\)'))
                    if ($match.Success) {
                        $position=0
                        foreach ($arg in ($match.Groups[1].Value -split ',')) {
                            $parts=$arg.Trim() -split '\s+',3
                            if ($parts.Count -ge 2) {
                                $parameter=@($method.Parameters | Where-Object Name -eq $parts[1]) | Select-Object -First 1
                                if (!$parameter) { $parameter=$method.Parameters.AddNew($parts[1],$parts[0]) }
                                $parameter.Position=$position
                                if ($parts.Count -eq 3) { $parameter.Default=$parts[2].TrimStart('=',' ') }
                                [void]$parameter.Update()
                                $position++
                            }
                        }
                    }
                }
                $obj.right=$obj.left+600
            } else { $width=$obj.right-$obj.left; $obj.left=850; $obj.right=850+$width }
            [void]$obj.Update()
        }
    }
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
    Write-Output ('REFINED '+$m.code)
}
