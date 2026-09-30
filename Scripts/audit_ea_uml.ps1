# Read-only audit: no model elements or diagram settings are changed.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach($m in $manifest | Where-Object {$_.code -match '^G0[78]-01$|^N0[123]-0[3457]'}){
    $d=$repo.GetDiagramByID($m.diagramId)
    Write-Output ('DIAGRAM '+$m.code)
    foreach($o in $d.DiagramObjects){
        $e=$repo.GetElementByID($o.ElementID)
        if($e.Name -in @('Persona','Componente') -or $e.Type -eq 'Interface'){
            [pscustomobject]@{element=$e.Name;type=$e.Type;abstract=$e.Abstract;stereotype=$e.Stereotype;style=$o.Style}|ConvertTo-Json -Compress
        }
    }
    foreach($dl in $d.DiagramLinks){
        $c=$repo.GetConnectorByID($dl.ConnectorID)
        [pscustomobject]@{source=$repo.GetElementByID($c.ClientID).Name;target=$repo.GetElementByID($c.SupplierID).Name;type=$c.Type;direction=$c.Direction;stereotype=$c.Stereotype;sourceCard=$c.ClientEnd.Cardinality;targetCard=$c.SupplierEnd.Cardinality;sourceNav=$c.ClientEnd.Navigable;targetNav=$c.SupplierEnd.Navigable;sourceAggregation=$c.ClientEnd.Aggregation;targetAggregation=$c.SupplierEnd.Aggregation}|ConvertTo-Json -Compress
    }
}
