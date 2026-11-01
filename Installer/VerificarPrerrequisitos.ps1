[CmdletBinding()]
param(
    [string]$ServerInstance = '.',
    [string]$DownloadDirectory = "$env:USERPROFILE\Downloads\Perfumeria-Prerrequisitos",
    [switch]$InstallDotNet,
    [switch]$CheckSqlOnly,
    [switch]$DiscoverSqlInstances,
    [string]$ResultPath,
    [string]$LogPath = "$env:TEMP\Perfumeria-Prerrequisitos.log"
)

$ErrorActionPreference = 'Stop'
$dotNetMinimumRelease = 461808
$dotNetUrl = 'https://go.microsoft.com/fwlink/?LinkID=863265'
$sqlDownloadPage = 'https://www.microsoft.com/es-es/sql-server/sql-server-downloads'

function Write-PrerequisiteLog {
    param([string]$Message)
    Add-Content -LiteralPath $LogPath -Value ("{0:yyyy-MM-dd HH:mm:ss} {1}" -f (Get-Date), $Message) -Encoding UTF8
}

function Test-DotNet472 {
    $paths = @(
        'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full',
        'HKLM:\SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\v4\Full'
    )
    foreach ($path in $paths) {
        $release = Get-ItemPropertyValue -LiteralPath $path -Name Release -ErrorAction SilentlyContinue
        if ($null -ne $release -and [int]$release -ge $dotNetMinimumRelease) {
            Write-PrerequisiteLog ".NET Framework detectado. Release=$release."
            return $true
        }
    }
    Write-PrerequisiteLog '.NET Framework 4.7.2 o superior no detectado.'
    return $false
}

function Get-SqlInstanceInfo {
    param([string]$Instance, [int]$ConnectionTimeout = 6)
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $Instance
    $builder['Initial Catalog'] = 'master'
    $builder['Integrated Security'] = $true
    $builder['Connect Timeout'] = $ConnectionTimeout
    $builder['TrustServerCertificate'] = $true
    $connection = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 6
        $command.CommandText = @"
SELECT CONVERT(int, SERVERPROPERTY('ProductMajorVersion')),
       CONVERT(int, SERVERPROPERTY('EngineEdition')),
       CASE WHEN DB_ID(N'Trabajo Practico Final') IS NOT NULL THEN 1 ELSE 0 END;
"@
        $reader = $command.ExecuteReader()
        try {
            if ($reader.Read()) {
                return [pscustomobject]@{ Instance = $Instance; MajorVersion = $reader.GetInt32(0);
                    IsExpress = $reader.GetInt32(1) -eq 4; HasApplicationDatabase = $reader.GetInt32(2) -eq 1 }
            }
        }
        finally { $reader.Dispose(); $command.Dispose() }
    }
    catch {
        Write-PrerequisiteLog "No se pudo conectar a SQL Server '$Instance': $($_.Exception.Message)"
        return $null
    }
    finally {
        $connection.Dispose()
    }
}

function Get-SqlMajorVersion {
    param([string]$Instance)
    $info = Get-SqlInstanceInfo -Instance $Instance
    if ($null -ne $info) { return $info.MajorVersion }
    return $null
}

function Get-LocalSqlInventory {
    # Read both registry views: Inno Setup can launch 32-bit PowerShell on 64-bit Windows.
    $instances = @()
    $localDbInstalled = $false
    $complete = $true
    foreach ($view in @([Microsoft.Win32.RegistryView]::Registry64, [Microsoft.Win32.RegistryView]::Registry32)) {
        if ($view -eq [Microsoft.Win32.RegistryView]::Registry64 -and -not [Environment]::Is64BitOperatingSystem) { continue }
        $registry = $null
        try {
            $registry = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, $view)
            $key = $registry.OpenSubKey('SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL')
            if ($key) { try { $instances += $key.GetValueNames() } finally { $key.Dispose() } }
            $key = $registry.OpenSubKey('SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions')
            if ($key) { try { $localDbInstalled = $localDbInstalled -or $key.GetSubKeyNames().Count -gt 0 } finally { $key.Dispose() } }
        }
        catch { $complete = $false; Write-PrerequisiteLog "No se pudo leer el inventario SQL ($view): $($_.Exception.Message)" }
        finally { if ($registry) { $registry.Dispose() } }
    }
    $services = @()
    try {
        $services = @(Get-Service -ErrorAction Stop | Where-Object { $_.Name -eq 'MSSQLSERVER' -or $_.Name.StartsWith('MSSQL$', [StringComparison]::OrdinalIgnoreCase) } |
            Select-Object Name, Status)
    }
    catch { $complete = $false; Write-PrerequisiteLog "No se pudieron consultar los servicios SQL: $($_.Exception.Message)" }
    return [pscustomobject]@{ Instances = @($instances | Sort-Object -Unique); Services = $services; LocalDbInstalled = $localDbInstalled; Complete = $complete }
}

function Get-SqlDiscoveryCandidates {
    param([Parameter(Mandatory = $true)]$Inventory)
    $names = @($Inventory.Instances)
    foreach ($service in $Inventory.Services) {
        if ($service.Name -eq 'MSSQLSERVER') { $names += 'MSSQLSERVER' }
        else { $names += $service.Name.Substring(6) }
    }
    foreach ($name in @($names | Sort-Object -Unique)) {
        $serviceName = $(if ($name -eq 'MSSQLSERVER') { 'MSSQLSERVER' } else { 'MSSQL$' + $name })
        $service = @($Inventory.Services | Where-Object { $_.Name -eq $serviceName })
        if ($service.Count -gt 0 -and [string]$service[0].Status -ne 'Running') { continue }
        $instance = $(if ($name -eq 'MSSQLSERVER') { '.' } else { '.\' + $name })
        if ($instance.Length -le 128 -and $instance -match '^[a-zA-Z0-9_.\\()\-]+$') { $instance }
    }
    if ($Inventory.LocalDbInstalled) { '(localdb)\MSSQLLocalDB' }
}

function Find-CompatibleSqlInstance {
    param([Parameter(Mandatory = $true)]$Inventory,
        [scriptblock]$Probe = { param($Instance) Get-SqlInstanceInfo -Instance $Instance -ConnectionTimeout 3 })
    $compatible = @()
    foreach ($candidate in @(Get-SqlDiscoveryCandidates $Inventory)) {
        $info = & $Probe $candidate
        if ($null -ne $info -and $info.MajorVersion -ge 16) { $compatible += $info }
    }
    # Prefer an existing application database to avoid sending an upgrade to an empty server.
    # Otherwise prefer a regular Express service, then other services, then per-user LocalDB.
    return $compatible | Sort-Object `
        @{Expression = { -not $_.HasApplicationDatabase }}, `
        @{Expression = { $_.Instance -like '(localdb)\*' }}, `
        @{Expression = { -not $_.IsExpress }}, `
        @{Expression = { $_.Instance -ne '.' }}, Instance | Select-Object -First 1
}

function Get-SqlFailureDiagnostic {
    param([string]$Instance, [Parameter(Mandatory = $true)]$Inventory)
    $localDb = $Instance -match '^\(localdb\)\\[^\\]+$'
    $parts = $Instance -split '\\', 2
    $hostName = $parts[0]
    $localNames = @('.', '(local)', 'localhost', $env:COMPUTERNAME)
    if ($env:USERDNSDOMAIN) { $localNames += "$env:COMPUTERNAME.$env:USERDNSDOMAIN" }
    $local = $localDb -or $hostName -in $localNames
    $code = 21
    $message = "No se pudo conectar a '$Instance'. Revise el nombre, la conexión y la configuración de SQL Server; este error no confirma que falte el motor."
    $serviceName = ''
    if ($local) {
        $available = @($Inventory.Instances | ForEach-Object { if ($_ -eq 'MSSQLSERVER') { '.' } else { '.\' + $_ } })
        foreach ($service in $Inventory.Services) {
            $available += $(if ($service.Name -eq 'MSSQLSERVER') { '.' } else { '.\' + $service.Name.Substring(6) })
        }
        if ($Inventory.LocalDbInstalled) { $available += '(localdb)\MSSQLLocalDB' }
        $suggestion = (@($available | Sort-Object -Unique | Select-Object -First 4) -join ', ')
        if ($localDb) {
            if ($Inventory.LocalDbInstalled) {
                $message = "LocalDB está instalado, pero '$Instance' no respondió. Revise el nombre con SqlLocalDB info y use la misma cuenta de Windows que creó la instancia. LocalDB no utiliza un servicio de Windows."
            }
            elseif ($suggestion) {
                $code = 23
                $message = "No se detectó LocalDB. Hay otras instancias locales: $suggestion. Indique una de ellas o instale LocalDB desde Microsoft."
            }
            elseif ($Inventory.Complete) {
                $code = 20
                $message = 'Motor de base de datos no instalado: no se detectó SQL Server ni LocalDB. Pulse «Descargar SQL Server» e instale Express 2022 o superior desde Microsoft.'
            }
        }
        else {
            $instanceName = $(if ($parts.Count -eq 1) { 'MSSQLSERVER' } else { $parts[1] })
            $expectedService = $(if ($instanceName -eq 'MSSQLSERVER') { 'MSSQLSERVER' } else { 'MSSQL$' + $instanceName })
            $service = @($Inventory.Services | Where-Object { $_.Name -eq $expectedService } | Select-Object -First 1)
            if ($service.Count -gt 0) {
                if ([string]$service[0].Status -ne 'Running') {
                    $code = 24
                    $serviceName = $expectedService
                    $message = "SQL Server está instalado, pero el servicio '$expectedService' no está en ejecución. Pulse «Abrir servicios», inícielo o reanúdelo y vuelva a pulsar Siguiente. No necesita reinstalar el motor."
                }
                else {
                    $message = "El servicio SQL Server está en ejecución, pero '$Instance' no acepta la conexión. Revise el nombre y la configuración de la instancia. No es necesario descargar otro motor."
                }
            }
            elseif ($instanceName -in $Inventory.Instances) {
                $code = 23
                $message = "La instancia '$Instance' está registrada, pero no se encontró su servicio '$expectedService'. Revise o repare esa instalación de SQL Server; luego vuelva a comprobarla."
            }
            elseif ($suggestion) {
                $code = 23
                $message = "No se encontró una instancia utilizable con el nombre '$Instance'. Instancias detectadas: $suggestion. Corrija el nombre y pulse Siguiente; se verificará la versión elegida."
            }
            elseif ($Inventory.Complete) {
                $code = 20
                $message = 'Motor de base de datos no instalado: no se detectó SQL Server ni LocalDB. Pulse «Descargar SQL Server» e instale Express 2022 o superior desde Microsoft.'
            }
        }
    }
    return [pscustomobject]@{ Code = $code; Message = $message; ServiceName = $serviceName }
}

function Complete-PrerequisiteCheck {
    param([int]$Code, [string]$Message, [string]$ServiceName = '', [string]$SelectedInstance = '')
    Write-PrerequisiteLog $Message
    if ($ResultPath) {
        $cleanMessage = $Message.Replace("`r", ' ').Replace("`n", ' ')
        # Windows INI APIs used by Inno Setup expect UTF-16 for accented text.
        Set-Content -LiteralPath $ResultPath -Encoding Unicode -Value @('[Prerequisites]', "Message=$cleanMessage", "Service=$ServiceName", "SelectedInstance=$SelectedInstance", "DownloadUrl=$sqlDownloadPage")
    }
    exit $Code
}

function Save-MicrosoftDownload {
    param([string]$Uri, [string]$Destination)
    New-Item -ItemType Directory -Path (Split-Path -Parent $Destination) -Force | Out-Null
    Remove-Item -LiteralPath $Destination -Force -ErrorAction SilentlyContinue
    Write-PrerequisiteLog "Descargando $Uri en $Destination."
    try {
        Start-BitsTransfer -Source $Uri -Destination $Destination -DisplayName 'Prerrequisito Perfumeria' -Description 'Descarga oficial de Microsoft'
    }
    catch {
        Write-PrerequisiteLog "BITS no pudo completar la descarga; se intenta HTTPS directo: $($_.Exception.Message)"
        Invoke-WebRequest -UseBasicParsing -Uri $Uri -OutFile $Destination
    }

    $signature = Get-AuthenticodeSignature -LiteralPath $Destination
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Microsoft') {
        Remove-Item -LiteralPath $Destination -Force -ErrorAction SilentlyContinue
        throw 'La descarga no posee una firma digital válida de Microsoft.'
    }
}

# Tests can dot-source the production diagnostic functions without running downloads or exiting.
if ($MyInvocation.InvocationName -eq '.') { return }

try {
    New-Item -ItemType File -Path $LogPath -Force | Out-Null
    Write-PrerequisiteLog "Inicio. Instancia solicitada: $ServerInstance"

    if ($DiscoverSqlInstances) {
        $inventory = Get-LocalSqlInventory
        $selected = Find-CompatibleSqlInstance -Inventory $inventory
        if ($null -ne $selected) {
            $reason = $(if ($selected.HasApplicationDatabase) { 'Ya contiene la base de Perfumería.' } else { 'Conexión y versión verificadas.' })
            Complete-PrerequisiteCheck -Code 0 -Message "Se seleccionó '$($selected.Instance)' (SQL Server $($selected.MajorVersion).x). $reason Puede cambiar la instancia antes de continuar." -SelectedInstance $selected.Instance
        }
        $diagnostic = Get-SqlFailureDiagnostic -Instance '.' -Inventory $inventory
        if ($diagnostic.Code -eq 20) { Complete-PrerequisiteCheck 20 $diagnostic.Message }
        Complete-PrerequisiteCheck 23 'No se encontró una instancia accesible con SQL Server 2022 o superior. Indique una instancia y pulse Siguiente para revisar su versión y el estado del servicio.'
    }

    if ([string]::IsNullOrWhiteSpace($ServerInstance) -or $ServerInstance.Length -gt 128 -or $ServerInstance -notmatch '^[a-zA-Z0-9_.\\()\-]+$') {
        Complete-PrerequisiteCheck 26 'Ingrese un nombre de instancia válido, por ejemplo ., .\SQLEXPRESS o (localdb)\MSSQLLocalDB.'
    }

    if (-not $CheckSqlOnly -and -not (Test-DotNet472)) {
        if (-not $InstallDotNet) {
            Write-PrerequisiteLog 'Falta .NET Framework y no se solicitó instalación automática.'
            Complete-PrerequisiteCheck 10 'Falta .NET Framework 4.7.2 o superior. Vuelva a ejecutar el instalador para descargarlo e instalarlo.'
        }

        $dotNetInstaller = Join-Path $DownloadDirectory 'NDP472-KB4054530-x86-x64-AllOS-ENU.exe'
        Save-MicrosoftDownload -Uri $dotNetUrl -Destination $dotNetInstaller
        Write-PrerequisiteLog 'Instalando .NET Framework silenciosamente.'
        $process = Start-Process -FilePath $dotNetInstaller -ArgumentList '/q /norestart' -Wait -PassThru -WindowStyle Hidden
        if ($process.ExitCode -in @(1641, 3010)) {
            Write-PrerequisiteLog 'La instalación de .NET requiere reiniciar Windows.'
            Complete-PrerequisiteCheck 30 '.NET Framework fue instalado. Reinicie Windows y vuelva a ejecutar el instalador para continuar.'
        }
        if ($process.ExitCode -ne 0 -or -not (Test-DotNet472)) {
            throw "La instalación de .NET Framework finalizó con código $($process.ExitCode)."
        }
    }

    $sqlMajor = Get-SqlMajorVersion -Instance $ServerInstance
    if ($null -eq $sqlMajor) {
        $diagnostic = Get-SqlFailureDiagnostic -Instance $ServerInstance -Inventory (Get-LocalSqlInventory)
        Complete-PrerequisiteCheck $diagnostic.Code $diagnostic.Message $diagnostic.ServiceName
    }

    if ($sqlMajor -lt 16) {
        Write-PrerequisiteLog "La instancia responde, pero su versión mayor es $sqlMajor; se requiere 16 o superior."
        Complete-PrerequisiteCheck 22 "La instancia responde, pero es SQL Server $sqlMajor.x. Seleccione una instancia 2022 (16.x) o superior, o instale Express desde «Descargar SQL Server»."
    }

    Write-PrerequisiteLog "Prerrequisitos verificados: .NET Framework 4.7.2+ y SQL Server $sqlMajor.x (minimo 16.x)."
    Complete-PrerequisiteCheck 0 "Conexión correcta con SQL Server $sqlMajor.x. Puede continuar."
}
catch {
    Write-PrerequisiteLog "ERROR: $($_.Exception.Message)"
    Complete-PrerequisiteCheck 99 'No se pudo completar la verificación. Revise Perfumeria-Prerrequisitos.log; no se modificó la base de datos.'
}
