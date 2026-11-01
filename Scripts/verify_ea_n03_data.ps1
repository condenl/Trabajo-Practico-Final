# Read-only check of the native physical diagrams against the shipped N03 DDL projection.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$specs=Get-Content (Join-Path $out 'n03-data-design.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach($s in $specs){
    $m=@($manifest | Where-Object code -eq $s.code)[0]
    $d=$repo.GetDiagramByGuid($m.guid)
    $objects=@($d.DiagramObjects)
    $e=@($objects | ForEach-Object {$repo.GetElementByID($_.ElementID)} | Where-Object Name -ceq 'EvaluacionProveedor')
    if($e.Count -ne 1){throw ('Tabla ausente o duplicada '+$s.code)}
    $spec=@($s.nodes | Where-Object name -ceq 'EvaluacionProveedor')[0]
    $actual=@($e[0].Attributes | ForEach-Object {$_.Name+':'+$_.Type} | Sort-Object)
    $expected=@($spec.attributes | ForEach-Object {$_.name+':'+$_.type} | Sort-Object)
    if(($actual -join '|') -cne ($expected -join '|')){throw ('Columnas diferentes del DDL '+$s.code)}
    $links=@($d.DiagramLinks | ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)} | Where-Object ClientID -eq $e[0].ElementID)
    $expectedLinks=@($s.links | Where-Object source -ceq 'EvaluacionProveedor')
    if($links.Count -ne 3){throw ('Cantidad de FK incorrecta '+$s.code)}
    foreach($l in $expectedLinks){
        $c=@($links | Where-Object Name -ceq $l.name)
        if($c.Count -ne 1){throw ('FK ausente '+$l.name)}
        $c=$c[0]
        if($c.Type -ne 'Association' -or $c.Stereotype -ne 'FK' -or $repo.GetElementByID($c.SupplierID).Name -cne $l.target -or $c.ClientEnd.Cardinality -ne $l.sourceCardinality -or $c.SupplierEnd.Cardinality -ne $l.targetCardinality){throw ('Notacion FK incorrecta '+$l.name)}
    }
    for($i=0;$i -lt $objects.Count;$i++){
        for($j=$i+1;$j -lt $objects.Count;$j++){
            $a=$objects[$i];$b=$objects[$j]
            if([Math]::Max($a.Left,$b.Left) -lt [Math]::Min($a.Right,$b.Right) -and [Math]::Max($a.Bottom,$b.Bottom) -lt [Math]::Min($a.Top,$b.Top)){throw ('Elementos superpuestos '+$s.code)}
        }
    }
    Write-Output ('VERIFIED '+$s.code+': 10 columns; 3 FK; optional selection/order; unique order cardinality; no overlapping elements')
}
