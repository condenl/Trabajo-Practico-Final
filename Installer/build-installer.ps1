param([string]$MSBuildPath, [string]$IsccPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if(-not $MSBuildPath){
    $vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if(Test-Path $vswhere){$MSBuildPath=(& $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1)}
}
if(-not $MSBuildPath -or -not (Test-Path $MSBuildPath)){throw 'Indicar -MSBuildPath con la ruta de MSBuild de Visual Studio.'}
& $MSBuildPath (Join-Path $root 'UI\UI.csproj') /restore /p:Configuration=Release /verbosity:minimal /nologo
if($LASTEXITCODE -ne 0){throw 'Fallo la compilacion Release.'}
$seedBin=Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Path $seedBin -Force | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
$references=@('System.dll','System.Core.dll','System.Data.dll','System.Configuration.dll')
$references+=@('BE','BLL','DAL','MPP','Servicios')|ForEach-Object {Join-Path $root ('UI\bin\Release\'+$_+'.dll')}
$arguments=@('/nologo','/target:exe',('/out:'+(Join-Path $seedBin 'SeedDemoActors.exe')))
$arguments+=$references|ForEach-Object {'/reference:'+$_}
$arguments+=Join-Path $root 'Scripts\SeedDemoActors.cs'
$arguments+=Join-Path $root 'Scripts\SeedDemoScenarios.cs'
$arguments+=Join-Path $root 'Scripts\SeedDemoTargetPolicy.cs'
& $compiler @arguments
if($LASTEXITCODE -ne 0){throw 'Fallo la compilacion del seed.'}
if(-not (Test-Path $IsccPath)){throw 'No se encontro Inno Setup 6. Indicar -IsccPath.'}
& $IsccPath (Join-Path $PSScriptRoot 'Perfumeria.iss')
if($LASTEXITCODE -ne 0){throw 'Fallo la compilacion del instalador.'}
