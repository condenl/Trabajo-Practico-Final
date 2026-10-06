# Integration test: creates isolated databases, never replaces an existing database.
param([string]$ServerInstance = '(localdb)\MSSQLLocalDB')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$stage = Join-Path $env:TEMP ('PerfumeriaLocalDb-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
Copy-Item (Join-Path $root 'UI\bin\Release\*.dll') $stage
Copy-Item (Join-Path $root 'UI\bin\Release\UI.exe.config') (Join-Path $stage 'UI.exe.config')
Copy-Item (Join-Path $root 'Installer\bin\SeedDemoActors.exe') $stage
$initializer = Join-Path $root 'Installer\InicializarBaseDatos.ps1'
$seed = Join-Path $stage 'SeedDemoActors.exe'
$stamp = [guid]::NewGuid().ToString('N').Substring(0, 12)
$databases = @("Perfumeria_LocalDb_Test_$stamp", "Perfumeria_LocalDb_Recovery_$stamp")
foreach ($database in $databases) {
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
    }
    finally { $connection.Dispose() }
    $arguments = @('-NoProfile', '-File', $initializer, '-ServerInstance', $ServerInstance,
        '-DatabaseName', $database, '-BackupPath', (Join-Path $root 'SQL\Trabajo Practico Final 3.bak'),
        '-MigrationDirectory', (Join-Path $root 'SQL'), '-ConfigPath', (Join-Path $stage 'UI.exe.config'),
        '-LogPath', (Join-Path $stage ($database + '.log')))
    if ($database -like '*_Recovery_*') {
        # Reproduce the old failure state: restore + migrations, no demo bootstrap.
        & powershell @arguments
        if ($LASTEXITCODE -ne 0) { throw 'Legacy-state setup failed' }
        Copy-Item (Join-Path $stage 'UI.exe.config') ($seed + '.config')
        & $seed --initialize-new $database
        if ($LASTEXITCODE -ne 0) { throw 'Explicit recovery of unused restored database failed' }
    }
    & powershell @arguments -SeedDemoActors
    if ($LASTEXITCODE -ne 0) { throw "Initialization failed: $database" }
    Get-Content (Join-Path $stage ($database + '.log'))
    & powershell @arguments -SeedDemoActors
    if ($LASTEXITCODE -ne 0) { throw "Idempotent retry failed: $database" }
    Get-Content (Join-Path $stage ($database + '.log'))
    Write-Output "PASS: $database (retained for inspection)"
}
Write-Output "STAGE=$stage"
