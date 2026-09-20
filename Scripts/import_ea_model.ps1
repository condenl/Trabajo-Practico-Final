param(
    [switch]$ExportOnly,
    [string]$ModelFile = 'model.json',
    [string]$PackageName = 'Iteraciones 1 y 2 - Procesos de perfumeria',
    [string]$ManifestFile = 'manifest.json'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'Documentacion\EnterpriseArchitect'
$model = Get-Content (Join-Path $output $ModelFile) -Raw -Encoding UTF8 | ConvertFrom-Json
$app = [Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')
$repo = $app.Repository
if ($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')) { throw 'El proyecto abierto no es diagramas.EAP del repositorio.' }
$project = $repo.GetProjectInterface()
$modelRoot = $repo.Models.GetAt(0)
$package = @($modelRoot.Packages | Where-Object Name -eq $packageName) | Select-Object -First 1
if (!$ExportOnly) {
    if ($package) { throw 'El paquete ya existe. Use -ExportOnly para no duplicar ni sobrescribir el trabajo.' }
    $backup = Join-Path $root ('Backup_EA_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
    New-Item -ItemType Directory -Path $backup | Out-Null
    Copy-Item -LiteralPath (Join-Path $root 'diagramas.EAP') -Destination (Join-Path $backup 'diagramas.EAP')
    $package = $modelRoot.Packages.AddNew($packageName, '')
    [void]$package.Update()
    foreach ($spec in $model) {
        $p = $package.Packages.AddNew($spec.code + ' ' + $spec.title, '')
        [void]$p.Update()
        $d = $p.Diagrams.AddNew($spec.title, $spec.kind)
        $d.Notes = $spec.notes
        $d.ShowDetails = 0
        [void]$d.Update()
        $elements = @{}
        foreach ($n in $spec.nodes) {
            $e = $p.Elements.AddNew($n.name, $n.kind)
            if ($n.stereotype) { $e.Stereotype = $n.stereotype }
            if ($n.notes) { $e.Notes = $n.notes }
            if ($n.subtype) { $e.Subtype = $n.subtype }
            [void]$e.Update()
            foreach ($a in $n.attributes) {
                $attribute = $e.Attributes.AddNew($a.name, $a.type)
                $attribute.Visibility = 'Public'
                if ($a.stereotype) { $attribute.Stereotype = $a.stereotype }
                if ($a.notes) { $attribute.Notes = $a.notes }
                [void]$attribute.Update()
            }
            foreach ($m in $n.methods) {
                $method = $e.Methods.AddNew($m.name, $m.type)
                $method.Visibility = 'Public'
                [void]$method.Update()
            }
            $elements[$n.key] = $e
            $obj = $d.DiagramObjects.AddNew(('l={0};r={1};t={2};b={3};' -f $n.x,($n.x+$n.w),$n.y,($n.y+$n.h)), '')
            $obj.ElementID = $e.ElementID
            if ($n.kind -eq 'ActivityPartition') { $obj.Sequence = 100 }
            [void]$obj.Update()
        }
        foreach ($l in $spec.links) {
            $c = $elements[$l.source].Connectors.AddNew($l.name, $l.kind)
            $c.SupplierID = $elements[$l.target].ElementID
            if ($l.stereotype) { $c.Stereotype = $l.stereotype }
            if ($l.kind -eq 'Sequence') {
                $c.DiagramID = $d.DiagramID
                $c.SequenceNo = $l.sequence
                $c.TransitionAction = 'Call'
                $c.TransitionEvent = 'Synchronous'
            }
            if ($l.sourceCardinality) { $c.ClientEnd.Cardinality = $l.sourceCardinality }
            if ($l.targetCardinality) { $c.SupplierEnd.Cardinality = $l.targetCardinality }
            [void]$c.Update()
            $dl = $d.DiagramLinks.AddNew('', '')
            $dl.ConnectorID = $c.ConnectorID
            if ($l.kind -eq 'ControlFlow') { $dl.Style = 'Mode=3;EOID=;SOID=;Color=-1;LWidth=0;' }
            [void]$dl.Update()
        }
        [void]$d.Update()
        Write-Output ('CREATED ' + $spec.code + ' diagram=' + $d.DiagramID)
    }
    $repo.RefreshModelView($package.PackageID)
}
$manifest = @()
$package.Packages.Refresh()
foreach ($p in $package.Packages) {
    $p.Diagrams.Refresh()
    foreach ($d in $p.Diagrams) {
        $code = $p.Name.Split(' ')[0]
        $file = Join-Path $output ($code + '.png')
        $repo.OpenDiagram($d.DiagramID)
        if (!$project.PutDiagramImageToFile($d.DiagramGUID, $file, 1)) { throw ('No se exporto ' + $code) }
        $manifest += [PSCustomObject]@{code=$code; title=$d.Name; diagramId=$d.DiagramID; guid=$d.DiagramGUID; file=$file}
        $repo.CloseDiagram($d.DiagramID)
        Write-Output ('EXPORTED ' + $code)
    }
}
$manifest | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $output $ManifestFile) -Encoding UTF8
$repo.OpenDiagram($manifest[0].diagramId)
Write-Output ('DONE ' + $manifest.Count + ' diagrams')
