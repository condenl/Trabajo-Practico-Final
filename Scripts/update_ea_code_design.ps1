param([string]$ModelFile='code-design-routed.json')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'Documentacion\EnterpriseArchitect'
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')) { throw 'Proyecto incorrecto' }
$specs=Get-Content (Join-Path $out $ModelFile) -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog=Get-Content (Join-Path $out 'class-catalog.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest=Get-Content (Join-Path $out 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifest+=Get-Content (Join-Path $out 'manifest-reconciliation.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$backup=Join-Path $root ('Backup_EA_CodeDesign_'+(Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Path $backup | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'diagramas.EAP') -Destination (Join-Path $backup 'diagramas.EAP')

function Add-Members($e,$n) {
    foreach($a in $n.attributes) {
        $attr=$e.Attributes.AddNew($a.name,$a.type)
        $attr.Visibility=if($a.visibility){$a.visibility}else{'Public'}
        [void]$attr.Update()
    }
    foreach($m in $n.methods) {
        $method=$e.Methods.AddNew($m.name,$m.type)
        $method.Visibility=$m.visibility
        if($m.static){$method.IsStatic=$true}
        if($m.abstract){$method.Abstract=$true}
        if($m.stereotype){$method.Stereotype=$m.stereotype}
        [void]$method.Update()
        $position=0
        foreach($param in $m.parameters) {
            $a=$method.Parameters.AddNew($param.name,$param.type)
            $a.Position=$position++; if($param.default){$a.Default=$param.default}; [void]$a.Update()
        }
    }
}

foreach($s in $specs) {
    $m=@($manifest | Where-Object code -eq $s.code)
    if($m.Count -ne 1){throw ('Manifest ambiguo '+$s.code)}
    $d=$repo.GetDiagramByID($m[0].diagramId);$p=$repo.GetPackageByID($d.PackageID)
    if($p.Name -notlike ($s.code+' *') -or $p.Diagrams.Count -ne 1){throw 'Paquete fuera del alcance generado'}
    $repo.CloseDiagram($d.DiagramID)
    # Preserve previous editable elements in a backup package, not just a binary copy.
    $arch=$p.Packages.AddNew('Version anterior '+(Get-Date -Format 'yyyyMMdd_HHmmss'),'');[void]$arch.Update()
    $prior=@($p.Elements)
    foreach($e in $prior){$e.PackageID=$arch.PackageID;[void]$e.Update()}
    $p.Elements.Refresh()
    for($i=$d.DiagramObjects.Count-1;$i -ge 0;$i--){$d.DiagramObjects.DeleteAt($i,$false)}
    for($i=$d.DiagramLinks.Count-1;$i -ge 0;$i--){$d.DiagramLinks.DeleteAt($i,$false)}
    $d.DiagramObjects.Refresh();$d.DiagramLinks.Refresh()
    $d.Notes=$s.notes;$d.ShowDetails=0;[void]$d.Update()
    $elements=@{}
    foreach($n in $s.nodes) {
        $e=$p.Elements.AddNew($n.name,$n.kind)
        if($n.stereotype){$e.Stereotype=$n.stereotype}
        if($n.notes){$e.Notes=$n.notes}
        if($n.abstract){$e.Abstract='1'}
        [void]$e.Update()
        if($n.kind -eq 'Object') {
            $type=$catalog.($n.classifier)
            $classifier=$p.Elements.AddNew($n.classifier,'Class');$classifier.Stereotype=$type.stereotype;$classifier.Notes=$type.notes;[void]$classifier.Update()
            Add-Members $classifier $type
            $e.ClassifierID=$classifier.ElementID; $e.Name='';[void]$e.Update()
        } else {Add-Members $e $n}
        $elements[$n.key]=$e
        $o=$d.DiagramObjects.AddNew(('l={0};r={1};t={2};b={3};' -f $n.x,($n.x+$n.w),$n.y,($n.y+$n.h)),'')
        $o.ElementID=$e.ElementID
        if($n.fontSize){$o.Style=('font=Arial;fontsz={0};bold=0;italic=0;ul=0;charset=0;pitch=34;' -f ($n.fontSize*10))}
        [void]$o.Update()
    }
    foreach($l in $s.links) {
        $c=$elements[$l.source].Connectors.AddNew($l.name,$l.kind);$c.SupplierID=$elements[$l.target].ElementID
        if($l.kind -eq 'Sequence'){$c.DiagramID=$d.DiagramID;$c.SequenceNo=$l.sequence;$c.TransitionAction='Call';$c.TransitionEvent='Synchronous'}
        if($l.sourceCardinality){$c.ClientEnd.Cardinality=$l.sourceCardinality}
        if($l.targetCardinality){$c.SupplierEnd.Cardinality=$l.targetCardinality}
        [void]$c.Update()
        $dl=$d.DiagramLinks.AddNew('','');$dl.ConnectorID=$c.ConnectorID
        if($l.path){$dl.Style='Mode=3;Color=-1;LWidth=0;';$dl.Path=$l.path}
        [void]$dl.Update()
    }
    foreach($f in $s.frames) {
        $e=$p.Elements.AddNew('','InteractionFragment');$e.Subtype=$f.kind;[void]$e.Update()
        foreach($guard in $f.guards){$partition=$e.Partitions.AddNew($guard,'Partition');$partition.Size=$f.size}
        [void]$e.Update()
        $lastCall=@($s.links | Where-Object sequence -eq $f.last)[0]
        $tail=if($lastCall.source -eq $lastCall.target){26}else{8}
        $top=135+35*($f.first-1)-25;$bottom=135+35*($f.last-1)+$tail
        $right=($s.nodes | Measure-Object x -Maximum).Maximum+300
        $o=$d.DiagramObjects.AddNew(('l=10;r={0};t={1};b={2};' -f $right,$top,$bottom),'');$o.ElementID=$e.ElementID;$o.Sequence=100;[void]$o.Update()
    }
    [void]$d.Update();$repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m[0].file,1)
    Write-Output ('UPDATED '+$s.code)
}
$repo.RefreshModelView(0)
