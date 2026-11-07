param([Parameter(Mandatory=$true)][string]$OutDirectory)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$repo=([Runtime.InteropServices.Marshal]::GetActiveObject('EA.App')).Repository
if($repo.ConnectionString -ne (Join-Path $root 'diagramas.EAP')){throw 'Proyecto incorrecto'}
New-Item -ItemType Directory -Force -Path $OutDirectory|Out-Null
$manifests=@('manifest.json','manifest-n03.json','manifest-reconciliation.json')
$manifest=@()
foreach($name in $manifests){$manifest+=Get-Content (Join-Path $root ('Documentacion\EnterpriseArchitect\'+$name)) -Raw -Encoding UTF8|ConvertFrom-Json}
$report=@()
foreach($code in @('N02-02','N03-01','N03-02','N03-06')){
    $entry=@($manifest|Where-Object code -eq $code)
    if($entry.Count -ne 1){throw ('Manifest ambiguo: '+$code)}
    $d=$repo.GetDiagramByGuid($entry[0].guid)
    $nodes=@($d.DiagramObjects|ForEach-Object {
        $o=$_;$e=$repo.GetElementByID($o.ElementID)
        @{id=$e.ElementID;guid=$e.ElementGUID;name=$e.Name;type=$e.Type;classifier=$e.ClassifierID;
          notes=$e.Notes;style=$o.Style;styleEx=$o.StyleEx;x=$o.left;y=-$o.top;w=$o.right-$o.left;h=$o.top-$o.bottom}
    })
    $links=@($d.DiagramLinks|ForEach-Object {
        $dl=$_;$c=$repo.GetConnectorByID($dl.ConnectorID);$props=@{}
        for($i=0;$i -lt $c.Properties.Count;$i++){
            $prop=$c.Properties.Item($i);$props[$prop.Name]=[string]$prop.Value
        }
        @{id=$c.ConnectorID;guid=$c.ConnectorGUID;name=$c.Name;kind=$c.Type;source=$c.ClientID;target=$c.SupplierID;
          sequence=$c.SequenceNo;stereotype=$c.Stereotype;styleEx=$c.StyleEx;path=$dl.Path;style=$dl.Style;hidden=$dl.IsHidden;
          event=$c.TransitionEvent;action=$c.TransitionAction;stateFlags=$c.StateFlags;properties=$props;misc0=$c.MiscData(0);misc1=$c.MiscData(1)}
    })
    $report+=@{code=$code;guid=$d.DiagramGUID;diagramId=$d.DiagramID;notes=$d.Notes;styleEx=$d.StyleEx;nodes=$nodes;links=$links}
    if(-not $repo.GetProjectInterface().PutDiagramImageToFile($d.DiagramGUID,(Join-Path $OutDirectory ($code+'.png')),1)){throw 'No se pudo exportar'}
    Write-Output ($code+': '+$nodes.Count+' elementos, '+$links.Count+' conectores')
}
$report|ConvertTo-Json -Depth 15|Set-Content (Join-Path $OutDirectory 'live-model.json') -Encoding UTF8
