[CmdletBinding()]
param(
    [string]$ServerInstance = '.\SQLEXPRESS',
    [string]$DatabaseName = 'Trabajo Practico Final',
    [Parameter(Mandatory = $true)]
    [string]$BackupPath,
    [Parameter(Mandatory = $true)]
    [string]$MigrationDirectory,
    [Parameter(Mandatory = $true)]
    [string]$ConfigPath,
    [string]$LogPath = "$PSScriptRoot\instalacion-base-datos.log",
    [switch]$SeedDemoActors
)

$ErrorActionPreference = 'Stop'

function Write-InstallLog {
    param([string]$Message)
    $line = "{0:yyyy-MM-dd HH:mm:ss} {1}" -f (Get-Date), $Message
    Add-Content -LiteralPath $LogPath -Value $line -Encoding UTF8
}

function New-SqlConnection {
    param([string]$Catalog)
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $ServerInstance
    $builder['Initial Catalog'] = $Catalog
    $builder['Integrated Security'] = $true
    $builder['Connect Timeout'] = 30
    $builder['TrustServerCertificate'] = $true
    return New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
}

function Invoke-SqlScalar {
    param(
        [string]$Catalog,
        [string]$CommandText,
        [hashtable]$Parameters = @{}
    )

    $connection = New-SqlConnection -Catalog $Catalog
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 120
        $command.CommandText = $CommandText
        foreach ($name in $Parameters.Keys) {
            [void]$command.Parameters.AddWithValue($name, $Parameters[$name])
        }
        return $command.ExecuteScalar()
    }
    finally {
        $connection.Dispose()
    }
}

function Invoke-SqlNonQuery {
    param(
        [string]$Catalog,
        [string]$CommandText,
        [hashtable]$Parameters = @{},
        [int]$Timeout = 120
    )

    $connection = New-SqlConnection -Catalog $Catalog
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandTimeout = $Timeout
        $command.CommandText = $CommandText
        foreach ($name in $Parameters.Keys) {
            [void]$command.Parameters.AddWithValue($name, $Parameters[$name])
        }
        [void]$command.ExecuteNonQuery()
    }
    finally {
        $connection.Dispose()
    }
}

function Invoke-SqlScript {
    param([string]$Path)

    Write-InstallLog "Aplicando migracion: $([IO.Path]::GetFileName($Path))"
    $script = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $batches = [regex]::Split($script, '(?im)^\s*GO\s*(?:--.*)?$')
    foreach ($batch in $batches) {
        if (-not [string]::IsNullOrWhiteSpace($batch)) {
            Invoke-SqlNonQuery -Catalog $DatabaseName -CommandText $batch -Timeout 300
        }
    }
}

function Restore-InitialDatabase {
    if (-not (Test-Path -LiteralPath $BackupPath -PathType Leaf)) {
        throw "No se encontro el respaldo inicial: $BackupPath"
    }

    $escapedBackup = $BackupPath.Replace("'", "''")
    $connection = New-SqlConnection -Catalog 'master'
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 120
        $command.CommandText = "RESTORE FILELISTONLY FROM DISK = N'$escapedBackup';"
        $reader = $command.ExecuteReader()
        $files = @()
        while ($reader.Read()) {
            $files += [PSCustomObject]@{
                LogicalName = [string]$reader['LogicalName']
                Type = [string]$reader['Type']
            }
        }
        $reader.Close()
    }
    finally {
        $connection.Dispose()
    }

    $dataFile = $files | Where-Object { $_.Type -eq 'D' } | Select-Object -First 1
    $logFile = $files | Where-Object { $_.Type -eq 'L' } | Select-Object -First 1
    if (-not $dataFile -or -not $logFile) {
        throw 'El respaldo no contiene archivos logicos de datos y log reconocibles.'
    }

    $paths = Invoke-SqlScalar -Catalog 'master' -CommandText @"
SELECT CONCAT(
    CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultDataPath')), N'|',
    CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultLogPath'))
);
"@
    $parts = ([string]$paths).Split('|')
    $dataDirectory = $parts[0]
    $logDirectory = if ($parts.Count -gt 1 -and $parts[1]) { $parts[1] } else { $dataDirectory }
    $safeFileName = ($DatabaseName -replace '[^A-Za-z0-9_-]', '_')
    $dataTarget = Join-Path $dataDirectory "$safeFileName.mdf"
    $logTarget = Join-Path $logDirectory "${safeFileName}_log.ldf"

    $escapedLogicalData = $dataFile.LogicalName.Replace("'", "''")
    $escapedLogicalLog = $logFile.LogicalName.Replace("'", "''")
    $escapedDataTarget = $dataTarget.Replace("'", "''")
    $escapedLogTarget = $logTarget.Replace("'", "''")
    $restoreSql = @"
RESTORE DATABASE [$($DatabaseName.Replace(']', ']]'))]
FROM DISK = @BackupPath
WITH MOVE N'$escapedLogicalData' TO N'$escapedDataTarget',
     MOVE N'$escapedLogicalLog' TO N'$escapedLogTarget',
     RECOVERY,
     REPLACE;
"@
    Invoke-SqlNonQuery -Catalog 'master' -CommandText $restoreSql -Parameters @{
        '@BackupPath' = $BackupPath
    } -Timeout 600
}

function Update-ApplicationConnection {
    if (-not (Test-Path -LiteralPath $ConfigPath -PathType Leaf)) {
        throw "No se encontro la configuracion de la aplicacion: $ConfigPath"
    }

    [xml]$config = Get-Content -LiteralPath $ConfigPath
    $entry = $config.configuration.connectionStrings.add | Where-Object { $_.name -eq 'MiConexionDB' }
    if (-not $entry) {
        throw "No existe la cadena de conexion 'MiConexionDB' en $ConfigPath"
    }

    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $ServerInstance
    $builder['Initial Catalog'] = $DatabaseName
    $builder['Integrated Security'] = $true
    $builder['TrustServerCertificate'] = $true
    $entry.connectionString = $builder.ConnectionString
    $config.Save($ConfigPath)
}

function Invoke-DemoSeed {
    param([string]$Mode)
    # Windows PowerShell 5 turns native stderr into ErrorRecords. Capture those
    # without aborting before the exit code and the complete output are logged.
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = & $seedExe $Mode $DatabaseName 2>&1
        $seedExit = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $previousPreference }
    foreach ($line in $output) { Write-InstallLog ([string]$line) }
    if ($seedExit -ne 0) { throw "Fallo de usuarios demo ($Mode, codigo $seedExit). Revisar el registro; no se sobrescriben usuarios existentes." }
}

try {
    $restoredNow = $false
    New-Item -ItemType File -Path $LogPath -Force | Out-Null
    Write-InstallLog "Inicio. Instancia SQL: $ServerInstance; base: $DatabaseName"

    $majorVersion = Invoke-SqlScalar -Catalog 'master' -CommandText "SELECT CONVERT(int, SERVERPROPERTY('ProductMajorVersion'));"
    if ([int]$majorVersion -lt 16) {
        throw "La aplicacion requiere SQL Server 16.x (SQL Server 2022) o superior. La instancia seleccionada informa version mayor $majorVersion."
    }

    if ($SeedDemoActors) {
        $seedExe = Join-Path (Split-Path $ConfigPath -Parent) 'SeedDemoActors.exe'
        if (-not (Test-Path -LiteralPath $seedExe -PathType Leaf)) { throw 'Falta SeedDemoActors.exe en la instalacion.' }
        Update-ApplicationConnection
        Copy-Item -LiteralPath $ConfigPath -Destination ($seedExe + '.config')
        # Reject unsupported demo targets before restoring or migrating a database.
        Invoke-DemoSeed -Mode '--check-target'
    }

    $exists = Invoke-SqlScalar -Catalog 'master' -CommandText 'SELECT DB_ID(@DatabaseName);' -Parameters @{
        '@DatabaseName' = $DatabaseName
    }
    if ($null -eq $exists -or $exists -is [DBNull]) {
        Write-InstallLog 'La base no existe. Restaurando respaldo inicial.'
        Restore-InitialDatabase
        $restoredNow = $true
    }
    else {
        Write-InstallLog 'La base ya existe. Se conserva y se aplican las migraciones idempotentes.'
    }

    $migrationNames = @(
        'Iteracion2_Aprovisionamiento.sql',
        'Iteracion2_Ventas_Fidelizacion.sql',
        'Iteracion3_Evaluacion_Proveedores.sql',
        'Iteracion3_Pulido_Demo.sql'
    )
    foreach ($migrationName in $migrationNames) {
        $migrationPath = Join-Path $MigrationDirectory $migrationName
        if (-not (Test-Path -LiteralPath $migrationPath -PathType Leaf)) {
            throw "No se encontro la migracion: $migrationPath"
        }
        Invoke-SqlScript -Path $migrationPath
    }

    Update-ApplicationConnection
    if ($SeedDemoActors) {
        $seedMode = if ($restoredNow) { '--initialize-new' } else { '--apply' }
        if ($restoredNow) { Write-InstallLog 'Inicializando DVH/DVV solo para la base nueva recien restaurada del respaldo de distribucion.' }
        foreach ($mode in @($seedMode, '--verify')) {
            Invoke-DemoSeed -Mode $mode
        }
    }
    Write-InstallLog 'Instalacion de base de datos finalizada correctamente.'
    exit 0
}
catch {
    Write-InstallLog "ERROR: $($_.Exception.Message)"
    Write-Error $_
    exit 1
}
