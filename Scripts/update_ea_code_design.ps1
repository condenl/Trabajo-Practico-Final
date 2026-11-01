param([string]$ModelFile='code-design-routed.json', [switch]$ClassesOnly, [string]$Codes='')
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
    if($ClassesOnly -and $s.kind -ne 'Logical'){continue}
    if($Codes -and $s.code -notin ($Codes -split ',')){continue}
    foreach($l in $s.links){
        if($l.kind -notin @('Association','Dependency','Generalization','Realization','Sequence')){throw ('Unsupported connector '+$l.kind)}
    }
    $m=@($manifest | Where-Object code -eq $s.code)
    if($m.Count -ne 1){throw ('Manifest ambiguo '+$s.code)}
    $d=$repo.GetDiagramByGuid($m[0].guid);$p=$repo.GetPackageByID($d.PackageID)
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
    $d.Notes=$s.notes;$d.ShowDetails=0
    if($s.kind -eq 'Sequence'){$d.StyleEx=($d.StyleEx -replace 'SuppressBrackets=[^;]*;','')+'SuppressBrackets=1;'}
    [void]$d.Update()
    $elements=@{}
    foreach($n in $s.nodes) {
        $e=$p.Elements.AddNew($n.name,$n.kind)
        if($n.stereotype){$e.Stereotype=$n.stereotype}
        if($n.notes){$e.Notes=$n.notes}
        if($n.abstract){$e.Abstract='1'}
        [void]$e.Update()
        if($n.namespace){$tag=$e.TaggedValues.AddNew('namespace',$n.namespace);[void]$tag.Update()}
        if($n.kind -eq 'Object') {
            $type=$catalog.($n.classifier)
            $classifier=$p.Elements.AddNew($n.classifier,'Class');$classifier.Stereotype=$type.stereotype;$classifier.Notes=$type.notes;[void]$classifier.Update()
            Add-Members $classifier $type
            $e.ClassifierID=$classifier.ElementID; $e.Name='';[void]$e.Update()
        } else {Add-Members $e $n}
        $elements[$n.key]=$e
        $o=$d.DiagramObjects.AddNew(('l={0};r={1};t={2};b={3};' -f $n.x,($n.x+$n.w),$n.y,($n.y+$n.h)),'')
        $o.ElementID=$e.ElementID
        if($n.fontSize){
            # Let EA render abstract classifier names itself, without italicizing concrete members.
            $o.Style=('font=Arial;fontsz={0};bold=0;ul=0;charset=0;pitch=34;' -f ($n.fontSize*10))
        }
        [void]$o.Update()
    }
    $realizationGuids=@()
    foreach($l in $s.links) {
        # EA 12.1 can reject Realization.Update through COM. Its native XMI importer
        # supports the actual connector type; convert only these recorded connectors below.
        $creationKind=if($l.kind -eq 'Realization'){'Dependency'}else{$l.kind}
        $c=$elements[$l.source].Connectors.AddNew($l.name,$creationKind);$c.SupplierID=$elements[$l.target].ElementID
        if($l.stereotype){$c.Stereotype=$l.stereotype}
        if($l.kind -eq 'Sequence'){$c.DiagramID=$d.DiagramID;$c.SequenceNo=$l.sequence;$c.TransitionAction='Call';$c.TransitionEvent='Synchronous'}
        if($l.sourceCardinality){$c.ClientEnd.Cardinality=$l.sourceCardinality}
        if($l.targetCardinality){$c.SupplierEnd.Cardinality=$l.targetCardinality}
        [void]$c.Update()
        if($l.kind -eq 'Sequence'){
            $c.Properties.Item('IsReturn').Value=[bool]$l.isReturn
            [void]$c.Update()
        }
        if($l.kind -eq 'Realization'){$realizationGuids+=$c.ConnectorGUID}
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
        $previous=@($s.links | Where-Object sequence -eq ($f.first-1))
        $head=if($previous.Count -and $previous[0].source -eq $previous[0].target){15}else{25}
        $top=135+35*($f.first-1)-$head;$bottom=135+35*($f.last-1)+$tail
        $right=($s.nodes | Measure-Object x -Maximum).Maximum+300
        $o=$d.DiagramObjects.AddNew(('l=10;r={0};t={1};b={2};' -f $right,$top,$bottom),'');$o.ElementID=$e.ElementID;$o.Sequence=100;[void]$o.Update()
    }
    [void]$d.Update()
    if($realizationGuids.Count){
        $diagramGuid=$d.DiagramGUID;$packageGuid=$p.PackageGUID
        $xmiPath=Join-Path $backup ($s.code+'.xmi')
        [void]$repo.GetProjectInterface().ExportPackageXMI($packageGuid,0,1,-1,1,0,$xmiPath)
        $xml=New-Object System.Xml.XmlDocument;$xml.Load($xmiPath)
        $ns=New-Object System.Xml.XmlNamespaceManager($xml.NameTable);$ns.AddNamespace('UML','omg.org/UML1.3')
        foreach($guid in $realizationGuids){
            $id='EAID_'+$guid.Trim('{}').Replace('-','_')
            $node=$xml.SelectSingleNode("//UML:Dependency[@xmi.id='$id']/UML:ModelElement.taggedValue/UML:TaggedValue[@tag='ea_type']",$ns)
            if(-not $node){throw ('XMI connector not found '+$guid)}
            $node.SetAttribute('value','Realisation')
        }
        $xml.Save($xmiPath)
        [void]$repo.GetProjectInterface().ImportPackageXMI($packageGuid,$xmiPath,1,0)
        $d=$repo.GetDiagramByGuid($diagramGuid)
        $m[0].diagramId=$d.DiagramID
        foreach($guid in $realizationGuids){
            if($repo.GetConnectorByGuid($guid).Type -ne 'Realisation'){throw ('Native realization import failed '+$guid)}
        }
    }
    $repo.ReloadDiagram($d.DiagramID)
    [void]$repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,$m[0].file,1)
    # XMI preserves GUIDs but EA can reassign local IDs. Keep both manifests current.
    foreach($manifestName in @('manifest.json','manifest-reconciliation.json')){
        $manifestPath=Join-Path $out $manifestName
        $saved=Get-Content $manifestPath -Raw -Encoding UTF8|ConvertFrom-Json
        foreach($entry in $saved){$entry.diagramId=$repo.GetDiagramByGuid($entry.guid).DiagramID}
        $saved|ConvertTo-Json -Depth 8|Set-Content $manifestPath -Encoding UTF8
    }
    Write-Output ('UPDATED '+$s.code)
}
$repo.RefreshModelView(0)
