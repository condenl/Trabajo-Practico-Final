[CmdletBinding()]
param(
    [string]$ServerInstance = '.',
    [string]$DownloadDirectory = "$env:USERPROFILE\Downloads\Perfumeria-Prerrequisitos",
    [switch]$InstallDotNet,
    [string]$LogPath = "$env:TEMP\Perfumeria-Prerrequisitos.log"
)

$ErrorActionPreference = 'Stop'
$dotNetMinimumRelease = 461808
$dotNetUrl = 'https://go.microsoft.com/fwlink/?LinkID=863265'
$sql2022Url = 'https://download.microsoft.com/download/5/1/4/5145fe04-4d30-4b85-b0d1-39533663a2f1/SQL2022-SSEI-Expr.exe'
$sql2022Page = 'https://www.microsoft.com/en-us/download/details.aspx?id=104781'

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

function Get-SqlMajorVersion {
    param([string]$Instance)
    $safeInstance = $Instance.Replace(';', '')
    $connection = New-Object System.Data.SqlClient.SqlConnection("Server=$safeInstance;Database=master;Integrated Security=True;Connect Timeout=6;TrustServerCertificate=True")
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT CONVERT(int, SERVERPROPERTY('ProductMajorVersion'));"
        return [int]$command.ExecuteScalar()
    }
    catch {
        Write-PrerequisiteLog "No se pudo conectar a SQL Server '$Instance': $($_.Exception.Message)"
        return $null
    }
    finally {
        $connection.Dispose()
    }
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

try {
    New-Item -ItemType File -Path $LogPath -Force | Out-Null
    Write-PrerequisiteLog "Inicio. Instancia solicitada: $ServerInstance"

    if (-not (Test-DotNet472)) {
        if (-not $InstallDotNet) {
            Write-PrerequisiteLog 'Falta .NET Framework y no se solicitó instalación automática.'
            exit 10
        }

        $dotNetInstaller = Join-Path $DownloadDirectory 'NDP472-KB4054530-x86-x64-AllOS-ENU.exe'
        Save-MicrosoftDownload -Uri $dotNetUrl -Destination $dotNetInstaller
        Write-PrerequisiteLog 'Instalando .NET Framework silenciosamente.'
        $process = Start-Process -FilePath $dotNetInstaller -ArgumentList '/q /norestart' -Wait -PassThru -WindowStyle Hidden
        if ($process.ExitCode -in @(1641, 3010)) {
            Write-PrerequisiteLog 'La instalación de .NET requiere reiniciar Windows.'
            exit 30
        }
        if ($process.ExitCode -ne 0 -or -not (Test-DotNet472)) {
            throw "La instalación de .NET Framework finalizó con código $($process.ExitCode)."
        }
    }

    $sqlMajor = Get-SqlMajorVersion -Instance $ServerInstance
    if ($null -eq $sqlMajor) {
        $sqlInstaller = Join-Path $DownloadDirectory 'SQL2022-SSEI-Expr.exe'
        try {
            Save-MicrosoftDownload -Uri $sql2022Url -Destination $sqlInstaller
            Write-PrerequisiteLog "SQL Server 2022 Express quedó descargado en $sqlInstaller."
            exit 20
        }
        catch {
            Write-PrerequisiteLog "No se pudo descargar el bootstrapper SQL 2022: $($_.Exception.Message). Página oficial: $sql2022Page"
            New-Item -ItemType Directory -Path $DownloadDirectory -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $DownloadDirectory 'DESCARGA_SQL_SERVER_2022.txt') -Value $sql2022Page -Encoding UTF8
            exit 21
        }
    }

    if ($sqlMajor -ne 16) {
        Write-PrerequisiteLog "La instancia responde, pero su versión mayor es $sqlMajor; se requiere 16."
        exit 22
    }

    Write-PrerequisiteLog 'Prerrequisitos verificados: .NET Framework 4.7.2+ y SQL Server 16.x.'
    exit 0
}
catch {
    Write-PrerequisiteLog "ERROR: $($_.Exception.Message)"
    Write-Error $_
    exit 99
}
