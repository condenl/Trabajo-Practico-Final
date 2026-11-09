param([switch]$VerifyOnly)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
$eap=Join-Path $root 'diagramas.EAP'
if($repo.ConnectionString -ne $eap){throw 'Proyecto EA incorrecto'}
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$stage=Join-Path $env:TEMP 'CU018-20261109'
New-Item -ItemType Directory -Path $stage -Force|Out-Null
$manifest=Get-Content (Join-Path $out 'manifest-n03.json') -Raw -Encoding UTF8|ConvertFrom-Json
$entry=@($manifest|Where-Object code -eq 'N03-01')
if($entry.Count -ne 1){throw 'Diagrama de casos de uso N03 ambiguo'}
$d=$repo.GetDiagramByGuid($entry[0].guid)
$text=Get-Content (Join-Path $root 'Documentacion\casos de uso\Caso_de_Uso_N03.md') -Raw -Encoding UTF8
$specs=@([regex]::Matches($text,'## (CU018\.\d+)[^\n]*\n(.*?)(?=\n## |\z)','Singleline'))
if($specs.Count -ne 4){throw 'Se esperan cuatro especificaciones CU018'}
function Graph {
    $nodes=@($d.DiagramObjects|ForEach-Object {
        $e=$repo.GetElementByID($_.ElementID)
        [ordered]@{guid=$e.ElementGUID;name=$e.Name;type=$e.Type;left=$_.left;right=$_.right;top=$_.top;bottom=$_.bottom}
    }|Sort-Object guid)
    $links=@($d.DiagramLinks|ForEach-Object {
        $c=$repo.GetConnectorByID($_.ConnectorID)
        [ordered]@{guid=$c.ConnectorGUID;name=$c.Name;type=$c.Type;source=$c.ClientID;target=$c.SupplierID;path=$_.Path;style=$_.Style}
    }|Sort-Object guid)
    [ordered]@{nodes=$nodes;links=$links}|ConvertTo-Json -Depth 8 -Compress
}
$before=Graph
if(-not $VerifyOnly){
    $backup=Join-Path $stage ('diagramas-antes-'+(Get-Date -Format 'yyyyMMdd_HHmmss')+'.eap')
    Copy-Item -LiteralPath $eap -Destination $backup
    if(-not $repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,(Join-Path $stage 'N03-01-before.png'),1)){throw 'No se pudo exportar la vista previa de EA'}
}
$report=@()
foreach($spec in $specs){
    $code=$spec.Groups[1].Value
    $notes=$spec.Groups[2].Value.Replace('<br>',"`n").Trim()
    $found=@($d.DiagramObjects|ForEach-Object {$repo.GetElementByID($_.ElementID)}|Where-Object {$_.Type -eq 'UseCase' -and $_.Name -match ('^'+[regex]::Escape($code)+'(?:\s|$)')})
    if($found.Count -ne 1){throw ('Caso de uso activo ambiguo: '+$code)}
    $e=$found[0];$changed=$e.Notes.Trim() -cne $notes
    if($changed -and -not $VerifyOnly){$e.Notes=$notes;if(-not $e.Update()){throw $e.GetLastError()}}
    $e=$repo.GetElementByGuid($e.ElementGUID)
    if($e.Notes.Trim() -cne $notes){throw ('La especificacion EA difiere: '+$code)}
    $report+=@{code=$code;guid=$e.ElementGUID;matchesCanonical=$true;updated=($changed -and -not $VerifyOnly)}
    Write-Output ('VERIFIED '+$code)
}
if((Graph) -cne $before){throw 'Cambio inesperado en el grafo o la geometria del diagrama'}
if(-not $VerifyOnly){
    if(-not $repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,(Join-Path $stage 'N03-01-after.png'),1)){throw 'No se pudo exportar la vista final de EA'}
    @{date='2026-11-09';diagram='N03-01';diagramGuid=$d.DiagramGUID;graphUnchanged=$true;specifications=$report}|
        ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'verification-cu018-ea.json') -Encoding UTF8
}
