# Integration/component tests on a NEW isolated database. Never changes UI/App.config.
param([string]$ServerInstance = 'DESKTOP-TFHVP49')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$stage = Join-Path $env:TEMP ('PerfumeriaUsabilidad-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
Copy-Item (Join-Path $root 'UI\bin\Release\*.dll') $stage
Copy-Item (Join-Path $root 'UI\bin\Release\UI.exe') $stage
Copy-Item (Join-Path $root 'UI\bin\Release\UI.exe.config') (Join-Path $stage 'UI.exe.config')
Copy-Item (Join-Path $root 'Installer\bin\SeedDemoActors.exe') $stage
Copy-Item (Join-Path $root 'SQL\Iteracion3_Pulido_Demo.sql') $stage
$database = 'Perfumeria_Usabilidad_Test_' + [guid]::NewGuid().ToString('N')
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Data Source'] = $ServerInstance
$builder['Initial Catalog'] = 'master'
$builder['Integrated Security'] = $true
$connection = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = 'SELECT DB_ID(@name)'
    [void]$command.Parameters.AddWithValue('@name', $database)
    if ($command.ExecuteScalar() -isnot [DBNull]) { throw "Test database already exists: $database" }
} finally { $connection.Dispose() }
& powershell -NoProfile -File (Join-Path $root 'Installer\InicializarBaseDatos.ps1') `
    -ServerInstance $ServerInstance -DatabaseName $database `
    -BackupPath (Join-Path $root 'SQL\Trabajo Practico Final 3.bak') -MigrationDirectory (Join-Path $root 'SQL') `
    -ConfigPath (Join-Path $stage 'UI.exe.config') -LogPath (Join-Path $stage 'setup.log') -SeedDemoActors
if ($LASTEXITCODE -ne 0) { Get-Content (Join-Path $stage 'setup.log'); throw 'Fixture initialization failed' }
$exe = Join-Path $stage 'TestUsabilidadOperativa.exe'
Copy-Item (Join-Path $stage 'UI.exe.config') ($exe + '.config')
$references = @('System.dll','System.Core.dll','System.Data.dll','System.Xml.dll','System.Configuration.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Windows.Forms.DataVisualization.dll')
$references += @('UI.exe','BE.dll','BLL.dll','DAL.dll','Servicios.dll') | ForEach-Object { Join-Path $stage $_ }
$arguments = @('/nologo','/target:exe',('/out:' + $exe))
$arguments += $references | ForEach-Object { '/reference:' + $_ }
$arguments += Join-Path $PSScriptRoot 'TestUsabilidadOperativa.cs'
& (Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe') @arguments
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& $exe --test
if ($LASTEXITCODE -ne 0) { throw "Tests failed; database retained: $database; stage: $stage" }
Write-Output "STAGE=$stage"
Write-Output "VISUAL_TEST_HOST=$exe"
