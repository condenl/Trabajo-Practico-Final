# Add the missing N03 BLL/DAL view and refresh hidden sequence classifiers.
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$backup=Join-Path $root ('Backup_EA_N03_'+(Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Path $backup|Out-Null
Copy-Item -LiteralPath (Join-Path $root 'diagramas.EAP') -Destination (Join-Path $backup 'diagramas.EAP')
$path=Join-Path $out 'manifest-reconciliation.json'
$manifest=Get-Content $path -Raw -Encoding UTF8|ConvertFrom-Json
if(-not ($manifest|Where-Object code -eq 'N03-05')){
    $source=$repo.GetDiagramByGuid(($manifest|Where-Object code -eq 'N03-04').guid)
    $parent=$repo.GetPackageByID($repo.GetPackageByID($source.PackageID).ParentID)
    $p=$parent.Packages.AddNew('N03-05 N03 - Clases de logica y persistencia de evaluacion','');[void]$p.Update()
    $d=$p.Diagrams.AddNew('N03 - Clases de logica y persistencia de evaluacion','Logical');[void]$d.Update()
    $manifest+=[pscustomobject]@{code='N03-05';title=$d.Name;diagramId=$d.DiagramID;guid=$d.DiagramGUID;file=(Join-Path $out 'N03-05.png')}
    $manifest|ConvertTo-Json -Depth 8|Set-Content $path -Encoding UTF8
}
$catalog=Get-Content (Join-Path $out 'class-catalog.json') -Raw -Encoding UTF8|ConvertFrom-Json
$specs=Get-Content (Join-Path $out 'code-design.json') -Raw -Encoding UTF8|ConvertFrom-Json
$all=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8|ConvertFrom-Json
$all+=$manifest
$count=0
foreach($s in $specs|Where-Object {$_.kind -eq 'Sequence' -and $_.code -ne 'N03-06'}){
    $d=$repo.GetDiagramByGuid(($all|Where-Object code -eq $s.code).guid)
    foreach($o in $d.DiagramObjects){
        $object=$repo.GetElementByID($o.ElementID)
        if($object.Type -ne 'Object' -or $object.ClassifierID -le 0){continue}
        $e=$repo.GetElementByID($object.ClassifierID)
        if($e.Name -notin @('UC_Aprovisionamiento','AprovisionamientoDAL','UC_Ventas')){continue}
        $n=$catalog.($e.Name)
        for($i=$e.Attributes.Count-1;$i -ge 0;$i--){$e.Attributes.DeleteAt($i,$false)}
        for($i=$e.Methods.Count-1;$i -ge 0;$i--){$e.Methods.DeleteAt($i,$false)}
        foreach($a in $n.attributes){$member=$e.Attributes.AddNew($a.name,$a.type);$member.Visibility=$a.visibility;[void]$member.Update()}
        foreach($m in $n.methods){
            $member=$e.Methods.AddNew($m.name,$m.type);$member.Visibility=$m.visibility;$member.IsStatic=[bool]$m.static;[void]$member.Update()
            $position=0
            foreach($param in $m.parameters){$p=$member.Parameters.AddNew($param.name,$param.type);$p.Position=$position++;if($param.default){$p.Default=$param.default};[void]$p.Update()}
        }
        $count++
    }
}
Write-Output "READY N03-05; refreshed hidden classifiers: $count"
