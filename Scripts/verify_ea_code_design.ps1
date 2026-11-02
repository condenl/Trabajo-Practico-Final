param([switch]$SequencesOnly, [string]$Codes='')
# Read-only verification of the active EA model against canonical C# signatures.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$specs=Get-Content (Join-Path $out 'code-design-routed.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if($SequencesOnly){$specs=@($specs|Where-Object kind -eq 'Sequence')}
if($Codes){$specs=@($specs|Where-Object code -in ($Codes -split ','))}
$catalog=Get-Content (Join-Path $out 'class-catalog.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$verifiedClasses=0;$verifiedMessages=0
foreach($s in $specs){
    $m=@($manifest | Where-Object code -eq $s.code)[0]
    $d=$repo.GetDiagramByID($m.diagramId)
    $elements=@($d.DiagramObjects | ForEach-Object {$repo.GetElementByID($_.ElementID)})
    foreach($n in $s.nodes){
        if($n.kind -eq 'Actor'){
            if(@($elements|Where-Object {$_.Type -eq 'Actor' -and $_.Name -ceq $n.name}).Count -ne 1){throw 'Actor ausente o duplicado'}
            continue
        }
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
        if($expected.abstract -and $actual.Abstract -ne '1'){throw ('Clase no abstracta '+$actual.Name)}
        # Native Interface already renders the UML keyword; XMI may remove the redundant stereotype.
        if($expected.kind -eq 'Interface' -and ($actual.Type -ne 'Interface' -or $actual.Stereotype -notin @('','interface'))){throw ('Interfaz mal identificada '+$actual.Name)}
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
    if($s.kind -eq 'Logical'){
        $connections=@($d.DiagramLinks | ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)})
        if($connections.Count -ne $s.links.Count){throw ('Cantidad de relaciones incorrecta '+$s.code)}
        $seen=@{}
        foreach($l in $s.links){
            $match=@($connections | Where-Object {
                ($_.Type -replace 'Realisation','Realization') -eq $l.kind -and $repo.GetElementByID($_.ClientID).Name -eq $l.source -and $repo.GetElementByID($_.SupplierID).Name -eq $l.target
            })
            if($match.Count -ne 1){throw ('Relacion ausente o duplicada '+$s.code+' '+$l.source+' '+$l.target)}
            $c=$match[0]
            if($l.kind -eq 'Association'){
                $key=@($l.source,$l.target | Sort-Object) -join '|'
                if($seen.ContainsKey($key)){throw ('Asociacion inversa duplicada '+$s.code)}
                $seen[$key]=$true
                if($c.ClientEnd.Cardinality -ne $l.sourceCardinality -or $c.SupplierEnd.Cardinality -ne $l.targetCardinality){throw 'Multiplicidad incorrecta'}
            }
            if($l.kind -eq 'Realization' -and $repo.GetElementByID($c.SupplierID).Type -ne 'Interface'){throw 'Realizacion sin interfaz receptora'}
        }
    }
    if($s.kind -eq 'Sequence'){
        $calls=@($d.DiagramLinks | ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)} | Where-Object Type -eq 'Sequence' | Sort-Object SequenceNo)
        if($calls.Count -ne $s.links.Count){throw ('Cantidad de mensajes incorrecta '+$s.code)}
        $stack=New-Object System.Collections.Stack
        for($i=0;$i -lt $calls.Count;$i++){
            $c=$calls[$i];$l=$s.links[$i]
            $te=$repo.GetElementByID($c.SupplierID);$se=$repo.GetElementByID($c.ClientID)
            $target=if($te.Type -eq 'Actor'){$te.Name}else{$repo.GetElementByID($te.ClassifierID).Name}
            $source=if($se.Type -eq 'Actor'){$se.Name}else{$repo.GetElementByID($se.ClassifierID).Name}
            if($c.Name -cne $l.name -or $target -cne $l.target -or $source -cne $l.source -or $c.SequenceNo -ne $l.sequence){throw ('Mensaje incorrecto '+$s.code)}
            if([bool]$c.Properties.Item('IsReturn').Value -ne [bool]$l.isReturn){throw ('Tipo de mensaje incorrecto '+$s.code)}
            if($l.isReturn){
                if(-not $stack.Count){throw 'Retorno sin llamada'}
                $call=$stack.Pop()
                if($l.replyTo -ne $call.callId -or $l.source -cne $call.target -or $l.target -cne $call.source -or $l.name -cne $call.returnType){throw ('Retorno no emparejado '+$s.code)}
            }else{
                if(-not $l.actorInteraction){
                    $method=@($catalog.($l.target).methods|Where-Object name -eq $l.name)
                    if($l.returnType -cnotin @($method|ForEach-Object type)){throw ('Tipo de retorno distinto al codigo '+$s.code)}
                }
                if($stack.Count -and $l.source -cne $stack.Peek().target){throw ('Llamada fuera de activacion '+$s.code)}
                $stack.Push($l)
            }
            $verifiedMessages++
        }
        if($stack.Count){throw 'Llamadas sin retorno'}
    }
    Write-Output ('VERIFIED '+$s.code)
}
Write-Output ('PASS: {0} class representations; {1} sequence messages; {2} diagrams' -f $verifiedClasses,$verifiedMessages,$specs.Count)
