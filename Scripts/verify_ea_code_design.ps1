# Read-only verification of the active EA model against canonical C# signatures.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$specs=Get-Content (Join-Path $out 'code-design-routed.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog=Get-Content (Join-Path $out 'class-catalog.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$verifiedClasses=0;$verifiedMessages=0
foreach($s in $specs){
    $m=@($manifest | Where-Object code -eq $s.code)[0]
    $d=$repo.GetDiagramByID($m.diagramId)
    $elements=@($d.DiagramObjects | ForEach-Object {$repo.GetElementByID($_.ElementID)})
    foreach($n in $s.nodes){
        if($n.kind -eq 'Object'){
            $e=@($elements | Where-Object {$_.Type -eq 'Object' -and $_.ClassifierID -gt 0 -and $repo.GetElementByID($_.ClassifierID).Name -eq $n.classifier})
            if($e.Count -ne 1){throw ('Clasificador incorrecto '+$s.code+' '+$n.classifier)}
            if($e[0].Name -ne ''){throw 'Lifeline must use its class name'}
            $actual=$repo.GetElementByID($e[0].ClassifierID);$expected=$catalog.($n.classifier)
        }else{
            $e=@($elements | Where-Object Name -eq $n.name)
            if($e.Count -ne 1){throw ('Clase no unica '+$s.code+' '+$n.name)}
            $actual=$e[0];$expected=$catalog.($n.name)
        }
        $a=@($actual.Attributes | ForEach-Object { $_.Name+':'+$_.Type } | Sort-Object)
        $b=@($expected.attributes | ForEach-Object { $_.name+':'+$_.type } | Sort-Object)
        if(($a -join '|') -cne ($b -join '|')){throw ('Atributos diferentes '+$actual.Name)}
        $a=@($actual.Methods | ForEach-Object {
            $params=@($_.Parameters | Sort-Object Position | ForEach-Object {$_.Name+':'+$_.Type}) -join ','
            $_.Name+'('+ $params +'):'+ $_.ReturnType+':'+$_.Visibility
        } | Sort-Object)
        $b=@($expected.methods | ForEach-Object {
            $params=@($_.parameters | ForEach-Object {$_.name+':'+$_.type}) -join ','
            $_.name+'('+ $params +'):'+ $_.type+':'+$_.visibility
        } | Sort-Object)
        if(($a -join '|') -cne ($b -join '|')){throw ('Operaciones diferentes '+$actual.Name)}
        $verifiedClasses++
    }
    if($s.kind -eq 'Sequence'){
        $calls=@($d.DiagramLinks | ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)} | Where-Object Type -eq 'Sequence' | Sort-Object SequenceNo)
        if($calls.Count -ne $s.links.Count){throw ('Cantidad de mensajes incorrecta '+$s.code)}
        for($i=0;$i -lt $calls.Count;$i++){
            $c=$calls[$i];$l=$s.links[$i]
            $target=$repo.GetElementByID($repo.GetElementByID($c.SupplierID).ClassifierID).Name
            if($c.Name -cne $l.name -or $target -cne $l.target){throw ('Mensaje incorrecto '+$s.code)}
            $verifiedMessages++
        }
    }
    Write-Output ('VERIFIED '+$s.code)
}
Write-Output ('PASS: {0} class representations; {1} sequence messages; {2} diagrams' -f $verifiedClasses,$verifiedMessages,$specs.Count)
