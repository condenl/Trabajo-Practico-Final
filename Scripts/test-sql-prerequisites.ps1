# Exercises the production diagnosis with fixture inventories; never stops real services.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$checker = Join-Path $root 'Installer\VerificarPrerrequisitos.ps1'
. $checker
$script:assertions = 0
function Assert-Result($Actual, $Expected, [string]$Text) {
    if ($Actual -ne $Expected) { throw "${Text}: expected=$Expected; actual=$Actual" }
    $script:assertions++
    Write-Output "PASS $Text"
}
function Inventory($Instances = @(), $Services = @(), $LocalDb = $false, $Complete = $true) {
    return [pscustomobject]@{ Instances = $Instances; Services = $Services; LocalDbInstalled = $LocalDb; Complete = $Complete }
}
function Service([string]$Name, [string]$Status) { return [pscustomobject]@{ Name = $Name; Status = $Status } }

$absent = Get-SqlFailureDiagnostic '.' (Inventory)
Assert-Result $absent.Code 20 'No motor ni instancias: ofrecer descarga'
Assert-Result ($absent.Message -match 'Motor de base de datos no instalado') $true 'Mensaje claro de motor ausente'
Assert-Result (Get-SqlFailureDiagnostic '(localdb)\MSSQLLocalDB' (Inventory)).Code 20 'LocalDB ausente y sin otro motor'
Assert-Result (Get-SqlFailureDiagnostic '.' (Inventory @() @() $false $false)).Code 21 'Inventario ilegible no implica motor ausente'

$express = Inventory @('SQLEXPRESS') @((Service 'MSSQL$SQLEXPRESS' 'Running'))
$wrong = Get-SqlFailureDiagnostic '.' $express
Assert-Result $wrong.Code 23 'Nombre equivocado con otra instancia instalada'
Assert-Result ($wrong.Message -match [regex]::Escape('.\SQLEXPRESS')) $true 'Se sugiere la instancia instalada'
Assert-Result (Get-SqlFailureDiagnostic '.\SQLEXPRESS' $express).Code 21 'Servicio activo sin conexión: revisar permisos, no reinstalar'
Assert-Result (Get-SqlFailureDiagnostic $env:COMPUTERNAME $express).Code 23 'Reconoce el nombre de la computadora local'
Assert-Result (Get-SqlFailureDiagnostic '.\SQLEXPRESS' (Inventory @('SQLEXPRESS'))).Code 23 'Instancia registrada sin servicio: reparar instalación'

foreach ($status in @('Stopped', 'Paused', 'StartPending')) {
    $stopped = Get-SqlFailureDiagnostic '.\SQLEXPRESS' (Inventory @('SQLEXPRESS') @((Service 'MSSQL$SQLEXPRESS' $status)))
    Assert-Result $stopped.Code 24 "Servicio ${status}: indicar Abrir servicios"
    Assert-Result $stopped.ServiceName 'MSSQL$SQLEXPRESS' "Servicio correcto para estado $status"
}
Assert-Result (Get-SqlFailureDiagnostic '.' (Inventory @('MSSQLSERVER') @((Service 'MSSQLSERVER' 'Stopped')))).Code 24 'Servicio de instancia predeterminada detenido'
Assert-Result (Get-SqlFailureDiagnostic '(localdb)\Personal' (Inventory @() @() $true)).Code 21 'LocalDB instalado sin conexión: no buscar servicio de Windows'
Assert-Result (Get-SqlFailureDiagnostic '.' (Inventory @() @() $true)).Code 23 'Solo LocalDB instalado: corregir instancia, no declarar motor ausente'
Assert-Result (Get-SqlFailureDiagnostic '(localdb)\MSSQLLocalDB' $express).Code 23 'LocalDB ausente pero existe Express: ofrecer alternativa'
Assert-Result (Get-SqlFailureDiagnostic 'SERVIDOR-REMOTO\SQL' (Inventory)).Code 21 'Fallo remoto no se diagnostica con inventario local'

function Info([string]$Instance, [int]$Version, [bool]$Express = $false, [bool]$HasDatabase = $false) {
    return [pscustomobject]@{ Instance = $Instance; MajorVersion = $Version; IsExpress = $Express; HasApplicationDatabase = $HasDatabase }
}
$all = Inventory @('MSSQLSERVER', 'SQLEXPRESS', 'OTRA') @() $true
$probes = @{
    '.' = (Info '.' 16)
    '.\SQLEXPRESS' = (Info '.\SQLEXPRESS' 17 $true)
    '.\OTRA' = (Info '.\OTRA' 16)
    '(localdb)\MSSQLLocalDB' = (Info '(localdb)\MSSQLLocalDB' 17 $true)
}
$probe = { param($Instance) $probes[$Instance] }
Assert-Result (Find-CompatibleSqlInstance $all $probe).Instance '.\SQLEXPRESS' 'Selección automática: Express compatible antes que otras instancias'
$probes['.'].HasApplicationDatabase = $true
Assert-Result (Find-CompatibleSqlInstance $all $probe).Instance '.' 'Base existente tiene prioridad sobre Express'
$probes['.'].HasApplicationDatabase = $false
$probes['.\SQLEXPRESS'].MajorVersion = 15
Assert-Result (Find-CompatibleSqlInstance $all $probe).Instance '.' 'Omite Express anterior a 16; prefiere servicio normal a LocalDB'
$probes['.'] = $null
Assert-Result (Find-CompatibleSqlInstance $all $probe).Instance '.\OTRA' 'Omite instancias que no responden'
$probes['.\OTRA'] = $null
Assert-Result (Find-CompatibleSqlInstance $all $probe).Instance '(localdb)\MSSQLLocalDB' 'Usa LocalDB compatible si no hay servicio utilizable'
$probes['(localdb)\MSSQLLocalDB'].HasApplicationDatabase = $true
$probes['.\OTRA'] = Info '.\OTRA' 16
Assert-Result (Find-CompatibleSqlInstance $all $probe).Instance '(localdb)\MSSQLLocalDB' 'Conserva base existente incluso si está en LocalDB'
$probes['.\OTRA'] = $null
$probes['(localdb)\MSSQLLocalDB'] = $null
Assert-Result ($null -eq (Find-CompatibleSqlInstance $all $probe)) $true 'No precarga una instancia inaccesible o incompatible'
Assert-Result (@(Get-SqlDiscoveryCandidates (Inventory)).Count) 0 'Sin motor no inventa nombres de instancia'
$stoppedInventory = Inventory @('SQLEXPRESS') @((Service 'MSSQL$SQLEXPRESS' 'Stopped'))
Assert-Result (@(Get-SqlDiscoveryCandidates $stoppedInventory).Count) 0 'No intenta conectar servicios detenidos durante la búsqueda'
Assert-Result (@(Get-SqlDiscoveryCandidates (Inventory @('SQLEXPRESS') @((Service 'MSSQL$SQLEXPRESS' 'Running')))).Count) 1 'Deduplica registro y servicio'

# Child-process smoke tests exercise actual SQL connection and UTF-16 INI handoff to Inno.
# These SELECTs do not restore or modify any application database.
$stage = Join-Path $env:TEMP ('PerfumeriaSqlPrerequisites-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
foreach ($case in @(
    @{ Instance = '.'; Code = 0; Name = 'SQL Server 16 o superior real' },
    @{ Instance = '(localdb)\MSSQLLocalDB'; Code = 0; Name = 'LocalDB 16 o superior real' },
    @{ Instance = '.\PERFUMERIA_INSTANCIA_INEXISTENTE'; Code = 23; Name = 'Instancia desconocida real' },
    @{ Instance = 'nombre;invalido'; Code = 26; Name = 'Entrada inválida no se conecta' }
)) {
    $ini = Join-Path $stage 'resultado.ini'
    $log = Join-Path $stage 'comprobacion.log'
    & powershell -NoProfile -ExecutionPolicy Bypass -File $checker -ServerInstance $case.Instance -CheckSqlOnly -ResultPath $ini -LogPath $log
    Assert-Result $LASTEXITCODE $case.Code $case.Name
    $bytes = [IO.File]::ReadAllBytes($ini)
    Assert-Result ($bytes[0] -eq 255 -and $bytes[1] -eq 254) $true ($case.Name + ': INI Unicode para Inno')
    Assert-Result ((Get-Content -LiteralPath $ini -Raw -Encoding Unicode) -match '(?m)^Message=.+') $true ($case.Name + ': mensaje escrito')
}
& powershell -NoProfile -ExecutionPolicy Bypass -File $checker -DiscoverSqlInstances -ResultPath $ini -LogPath $log
Assert-Result $LASTEXITCODE 0 'Descubrimiento real encuentra una instancia compatible'
$discovered = Get-Content -LiteralPath $ini -Raw -Encoding Unicode
Assert-Result ($discovered -match '(?m)^SelectedInstance=(.+)') $true 'Entrega el nombre elegido a Inno Setup'
$selectedInfo = Get-SqlInstanceInfo $Matches[1].Trim()
Assert-Result ($null -ne $selectedInfo -and $selectedInfo.MajorVersion -ge 16) $true 'La instancia precargada responde y cumple la versión mínima'
Write-Output "RESULTADO: $script:assertions verificaciones OK. Registros: $stage"
