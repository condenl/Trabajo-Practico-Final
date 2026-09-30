# Refine the UML representation of foreign keys; this does not create/change SQL constraints.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach($m in $manifest | Where-Object {$_.code -match '^G08-01$|^N0[123]-07'}){
    $d=$repo.GetDiagramByID($m.diagramId);$repo.CloseDiagram($d.DiagramID)
    foreach($dl in $d.DiagramLinks){
        $c=$repo.GetConnectorByID($dl.ConnectorID)
        if($c.Type -ne 'Association'){continue}
        if(-not $c.ClientEnd.Cardinality -or -not $c.SupplierEnd.Cardinality){throw ('Missing FK multiplicity '+$m.code)}
        $c.Stereotype='FK'
        $c.Direction='Unspecified'
        $c.Notes='Representacion UML de la referencia por clave foranea indicada en la etiqueta. Las restricciones ejecutables se definen en los scripts SQL; N03 es diseno propuesto.'
        [void]$c.Update()
    }
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
    Write-Output ('FK NOTATION '+$m.code)
}
