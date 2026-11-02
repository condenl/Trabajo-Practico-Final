# Read-only verification of the paths actually stored by EA, not just JSON routing proposals.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$specs=Get-Content (Join-Path $out 'code-design-routed.json') -Raw -Encoding UTF8|ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8|ConvertFrom-Json
function Points([string]$path){
    @($path.TrimEnd(';').Split(';')|ForEach-Object {
        $parts=$_.Split(':')
        [pscustomobject]@{x=[double]::Parse($parts[0],[Globalization.CultureInfo]::InvariantCulture);y=-[double]::Parse($parts[1],[Globalization.CultureInfo]::InvariantCulture)}
    })
}
function Intersects($a,$b,$c,$d){
    if($a.x -eq $b.x -and $c.x -eq $d.x){return $a.x -eq $c.x -and [Math]::Max([Math]::Min($a.y,$b.y),[Math]::Min($c.y,$d.y)) -le [Math]::Min([Math]::Max($a.y,$b.y),[Math]::Max($c.y,$d.y))}
    if($a.y -eq $b.y -and $c.y -eq $d.y){return $a.y -eq $c.y -and [Math]::Max([Math]::Min($a.x,$b.x),[Math]::Min($c.x,$d.x)) -le [Math]::Min([Math]::Max($a.x,$b.x),[Math]::Max($c.x,$d.x))}
    if($a.x -ne $b.x){return Intersects $c $d $a $b}
    return $c.y -ge [Math]::Min($a.y,$b.y) -and $c.y -le [Math]::Max($a.y,$b.y) -and $a.x -ge [Math]::Min($c.x,$d.x) -and $a.x -le [Math]::Max($c.x,$d.x)
}
$results=@()
foreach($s in $specs|Where-Object code -in @('N03-04','N03-05')){
    $entry=@($manifest|Where-Object code -eq $s.code)[0]
    $d=$repo.GetDiagramByGuid($entry.guid)
    $native=@($d.DiagramLinks)
    if($native.Count -ne $s.links.Count){throw 'Cantidad de relaciones diferente'}
    $routes=@()
    foreach($l in $s.links){
        $link=@($native|Where-Object {
            $c=$repo.GetConnectorByID($_.ConnectorID)
            $repo.GetElementByID($c.ClientID).Name -ceq $l.source -and $repo.GetElementByID($c.SupplierID).Name -ceq $l.target
        })
        if($link.Count -ne 1){throw 'Relacion ausente o duplicada'}
        $c=$repo.GetConnectorByID($link[0].ConnectorID)
        $kind=if($l.kind -eq 'Realization'){'Realisation'}else{$l.kind}
        if($c.Type -ne $kind){throw 'Cambio de tipo UML'}
        if(($l.sourceCardinality -and $c.ClientEnd.Cardinality -ne $l.sourceCardinality) -or ($l.targetCardinality -and $c.SupplierEnd.Cardinality -ne $l.targetCardinality)){throw 'Cambio de multiplicidad'}
        $actual=Points $link[0].Path;$expected=Points $l.path
        if($actual.Count -ne $expected.Count){throw 'Ruta no guardada por EA'}
        for($i=0;$i -lt $actual.Count;$i++){
            if([Math]::Abs($actual[$i].x-$expected[$i].x) -gt 1 -or [Math]::Abs($actual[$i].y-$expected[$i].y) -gt 1){throw 'Ruta distinta de la esperada'}
        }
        for($i=1;$i -lt $actual.Count;$i++){
            $a=$actual[$i-1];$b=$actual[$i]
            if($a.x -ne $b.x -and $a.y -ne $b.y){throw 'Segmento diagonal'}
            foreach($n in $s.nodes){
                $cross=if($a.x -eq $b.x){$a.x -gt $n.x -and $a.x -lt ($n.x+$n.w) -and [Math]::Max([Math]::Min($a.y,$b.y),$n.y) -lt [Math]::Min([Math]::Max($a.y,$b.y),($n.y+$n.h))}
                    else{$a.y -gt $n.y -and $a.y -lt ($n.y+$n.h) -and [Math]::Max([Math]::Min($a.x,$b.x),$n.x) -lt [Math]::Min([Math]::Max($a.x,$b.x),($n.x+$n.w))}
                if($cross){throw ('Una ruta atraviesa '+$n.name)}
            }
        }
        $routes+=,[pscustomobject]@{source=$l.source;target=$l.target;kind=$c.Type;points=$actual}
    }
    for($i=0;$i -lt $routes.Count;$i++){
        for($j=$i+1;$j -lt $routes.Count;$j++){
            $a=$routes[$i].points;$b=$routes[$j].points
            for($x=1;$x -lt $a.Count;$x++){for($y=1;$y -lt $b.Count;$y++){
                if(Intersects $a[$x-1] $a[$x] $b[$y-1] $b[$y]){throw 'Cruce o tramo compartido entre relaciones'}
            }}
        }
    }
    $results+=[pscustomobject]@{code=$s.code;relations=$routes.Count;intersections=0;sharedSegments=0;throughClasses=0;nativeRoutes=$routes}
    Write-Output ('VERIFIED '+$s.code+': no crossings, shared segments or paths through classes; UML types/cardinalities preserved')
}
$results|ConvertTo-Json -Depth 10|Set-Content (Join-Path $out 'verification-n03-routes.json') -Encoding UTF8
