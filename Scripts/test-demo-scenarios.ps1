# Creates one isolated database, then exercises the real BLL with each demo actor.
# Never runs destructive test cases against the development or installed database.
param([string]$ServerInstance = '(localdb)\MSSQLLocalDB')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$stage = Join-Path $env:TEMP ('PerfumeriaDemoScenarios-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
Copy-Item (Join-Path $root 'UI\bin\Release\*.dll') $stage
Copy-Item (Join-Path $root 'UI\bin\Release\UI.exe.config') (Join-Path $stage 'UI.exe.config')
Copy-Item (Join-Path $root 'Installer\bin\SeedDemoActors.exe') $stage
$database = 'Perfumeria_Demo_Test_' + [guid]::NewGuid().ToString('N')
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
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
$exe = Join-Path $stage 'TestDemoScenarios.exe'
Copy-Item (Join-Path $stage 'UI.exe.config') ($exe + '.config')
$references = @('System.dll','System.Core.dll','System.Data.dll','System.Xml.dll','System.Configuration.dll')
$references += @('BE','BLL','DAL','MPP','Servicios') | ForEach-Object { Join-Path $stage ($_ + '.dll') }
$arguments = @('/nologo','/target:exe',('/out:' + $exe))
$arguments += $references | ForEach-Object { '/reference:' + $_ }
$arguments += Join-Path $PSScriptRoot 'TestDemoScenarios.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& $exe $database (Join-Path $stage 'SeedDemoActors.exe')
if ($LASTEXITCODE -ne 0) { throw "Scenario tests failed; database retained: $database; stage: $stage" }
Write-Output "STAGE=$stage"
