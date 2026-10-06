param(
    [Parameter(Mandatory=$true)][string]$Database,
    [ValidateSet('check','apply','verify')][string]$Mode='check'
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$bin=Join-Path $root 'UI\bin\Release'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
foreach($name in @('BE','BLL','DAL','MPP','Servicios')){
    if(-not (Test-Path (Join-Path $bin ($name+'.dll')))){throw 'Compilar UI en Release antes de ejecutar la precarga.'}
}
$build=Join-Path ([IO.Path]::GetTempPath()) ('PerfumeriaDemoActors-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $build | Out-Null
foreach($name in @('BE','BLL','DAL','MPP','Servicios')){Copy-Item -LiteralPath (Join-Path $bin ($name+'.dll')) -Destination $build}
$exe=Join-Path $build 'SeedDemoActors.exe'
Copy-Item -LiteralPath (Join-Path $root 'UI\App.config') -Destination ($exe+'.config')
$references=@('System.dll','System.Core.dll','System.Data.dll','System.Configuration.dll')
$references+=@('BE','BLL','DAL','MPP','Servicios')|ForEach-Object {Join-Path $build ($_+'.dll')}
$arguments=@('/nologo','/target:exe',('/out:'+$exe))
$arguments+=$references|ForEach-Object {'/reference:'+$_}
$arguments+=Join-Path $PSScriptRoot 'SeedDemoActors.cs'
$arguments+=Join-Path $PSScriptRoot 'SeedDemoTargetPolicy.cs'
& $compiler @arguments
if($LASTEXITCODE -ne 0){throw 'No se pudo compilar la herramienta de precarga.'}
& $exe ('--'+$Mode) $Database
if($LASTEXITCODE -ne 0){throw 'La precarga/verificacion no finalizo correctamente. No se sobrescriben cuentas existentes.'}
