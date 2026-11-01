# Minimal UML object flows, preserving existing actions, decisions and partitions.
param([switch]$VerifyOnly)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$app=[Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')
$repo=$app.Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8|ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-n03.json') -Raw -Encoding UTF8|ConvertFrom-Json
$flows=@(
    @{code='N01-02';name='Orden de compra';from='Confeccionar orden preliminar';to='Contabilidad: revisar comparativa y presupuesto';x=337;y=598;w=180;h=45;file='model.json';source='draft';target='review'},
    @{code='N02-02';name='Membresía';from='Completar ficha y asignar membresía';to='Verificar beneficio vigente; sin membresía, tarifa general';x=395;y=915;w=130;h=35;file='model.json';source='register';target='benefit'},
    @{code='N03-02';name='Propuestas comparables';from='Consolidar propuestas comparables por producto';to='Analizar costos, márgenes e impacto financiero';x=600;y=598-125;w=190;h=55;file='model-n03.json';source='consolidate';target='cost'}
)
if(-not $VerifyOnly){
    $backup=Join-Path $root ('Backup_EA_ActivityObjects_'+(Get-Date -Format 'yyyyMMdd_HHmmss'))
    New-Item -ItemType Directory -Path $backup|Out-Null
    Copy-Item (Join-Path $root 'diagramas.EAP') (Join-Path $backup 'diagramas.EAP')
}
$report=@()
foreach($f in $flows){
    $m=@($manifest|Where-Object code -eq $f.code)[0];$d=$repo.GetDiagramByGuid($m.guid)
    $all=@($d.DiagramObjects|ForEach-Object {$repo.GetElementByID($_.ElementID)})
    $source=@($all|Where-Object Name -eq $f.from);$target=@($all|Where-Object Name -eq $f.to)
    if($source.Count -ne 1 -or $target.Count -ne 1){throw ('Activity not unique: '+$f.code)}
    $objects=@($all|Where-Object {$_.Name -eq $f.name -and $_.Type -eq 'Object'})
    if($objects.Count -gt 1){throw 'Duplicate object node'}
    if(-not $VerifyOnly){
        $repo.CloseDiagram($d.DiagramID)
        if($objects.Count){$e=$objects[0]}else{
            $e=$repo.GetPackageByID($d.PackageID).Elements.AddNew($f.name,'Object');[void]$e.Update()
            $o=$d.DiagramObjects.AddNew('','');$o.ElementID=$e.ElementID;[void]$o.Update()
        }
        $e.Notes='Objeto de negocio producido por «'+$f.from+'» y utilizado por «'+$f.to+'». Flujo de objetos UML; no representa un componente de software.';[void]$e.Update()
        $d.DiagramObjects.Refresh()
        $o=@($d.DiagramObjects|Where-Object ElementID -eq $e.ElementID)[0]
        $o.left=$f.x;$o.right=$f.x+$f.w;$o.top=-$f.y;$o.bottom=-($f.y+$f.h)
        $o.SetStyleEx('BCol','16777215');[void]$o.Update()
        # The object path replaces the direct edge visually. Preserve an existing
        # control connector in the model rather than deleting it from EA's cached collections.
        foreach($dl in $d.DiagramLinks){
            $edge=$repo.GetConnectorByID($dl.ConnectorID)
            if($edge.Type -eq 'ControlFlow' -and $edge.ClientID -eq $source[0].ElementID -and $edge.SupplierID -eq $target[0].ElementID){$dl.IsHidden=$true;[void]$dl.Update()}
        }
        foreach($pair in @(@{a=$source[0];b=$e},@{a=$e;b=$target[0]})){
            $matches=@($pair.a.Connectors|Where-Object {$_.Type -eq 'ObjectFlow' -and $_.ClientID -eq $pair.a.ElementID -and $_.SupplierID -eq $pair.b.ElementID})
            if(-not $matches.Count){
                $c=$pair.a.Connectors.AddNew('','ObjectFlow');$c.SupplierID=$pair.b.ElementID;[void]$c.Update()
                $dl=$d.DiagramLinks.AddNew('','');$dl.ConnectorID=$c.ConnectorID;$dl.Style='Mode=3;Color=-1;LWidth=0;';[void]$dl.Update()
            }
        }
        if($f.code -eq 'N02-02'){
            # Existing members bypass the registration/membership creation branch.
            foreach($dl in $d.DiagramLinks){
                $edge=$repo.GetConnectorByID($dl.ConnectorID)
                if($edge.Type -eq 'ControlFlow' -and $repo.GetElementByID($edge.ClientID).Name -eq 'Consultar pertenencia al programa' -and $edge.SupplierID -eq $target[0].ElementID){
                    $dl.Style='Mode=3;Color=-1;LWidth=0;';$dl.Path='542:-495;542:-995;';[void]$dl.Update()
                }
            }
        }
        $d.DiagramObjects.Refresh();$d.DiagramLinks.Refresh();[void]$d.Update()
        $repo.ReloadDiagram($d.DiagramID)
        [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m.file,1)
        # Keep the stored source models reproducible without regenerating unrelated views.
        $modelPath=Join-Path $out $f.file
        $models=Get-Content $modelPath -Raw -Encoding UTF8|ConvertFrom-Json
        $spec=@($models|Where-Object code -eq $f.code)[0];$key='object-flow-'+$f.source
        if(-not @($spec.nodes|Where-Object key -eq $key).Count){
            $spec.nodes+=@{key=$key;name=$f.name;kind='Object';x=$f.x;y=$f.y;w=$f.w;h=$f.h;notes=$e.Notes}
            $spec.links=@($spec.links|Where-Object {-not ($_.source -eq $f.source -and $_.target -eq $f.target -and $_.kind -eq 'ControlFlow')})
            $spec.links+=@{source=$f.source;target=$key;name='';kind='ObjectFlow'},@{source=$key;target=$f.target;name='';kind='ObjectFlow'}
            $models|ConvertTo-Json -Depth 30|Set-Content $modelPath -Encoding UTF8
        }
        else {
            $node=@($spec.nodes|Where-Object key -eq $key)[0]
            $node.x=$f.x;$node.y=$f.y;$node.w=$f.w;$node.h=$f.h
            $models|ConvertTo-Json -Depth 30|Set-Content $modelPath -Encoding UTF8
        }
    }
    $elements=@($d.DiagramObjects|ForEach-Object {$repo.GetElementByID($_.ElementID)})
    $object=@($elements|Where-Object {$_.Type -eq 'Object' -and $_.Name -eq $f.name})
    if($object.Count -ne 1){throw 'Object node absent or duplicated'}
    if($f.code -eq 'N02-02'){
        $position=@($d.DiagramObjects|Where-Object ElementID -eq $object[0].ElementID)[0]
        $lane=@($d.DiagramObjects|Where-Object {$repo.GetElementByID($_.ElementID).Type -eq 'ActivityPartition' -and $repo.GetElementByID($_.ElementID).Name -eq 'Personal de Ventas'})[0]
        if($position.left -lt $lane.left -or $position.right -gt $lane.right){throw 'Membership is outside the Sales partition'}
    }
    $links=@($d.DiagramLinks|Where-Object {-not $_.IsHidden}|ForEach-Object {$repo.GetConnectorByID($_.ConnectorID)})
    foreach($pair in @(@($source[0].ElementID,$object[0].ElementID),@($object[0].ElementID,$target[0].ElementID))){
        if(@($links|Where-Object {$_.Type -eq 'ObjectFlow' -and $_.ClientID -eq $pair[0] -and $_.SupplierID -eq $pair[1]}).Count -ne 1){throw 'Object flow absent or duplicated'}
    }
    if(@($links|Where-Object {$_.Type -eq 'ControlFlow' -and $_.ClientID -eq $source[0].ElementID -and $_.SupplierID -eq $target[0].ElementID}).Count){throw 'Redundant direct control flow'}
    $report+=@{diagram=$f.code;object=$f.name;producer=$f.from;consumer=$f.to;objectFlows=2}
    Write-Output ('VERIFIED '+$f.code+' '+$f.name)
}
if(-not $VerifyOnly){$report|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'verification-activity-objects.json') -Encoding UTF8}
