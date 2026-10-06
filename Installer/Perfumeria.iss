#define MyAppName "Perfumeria"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Trabajo Practico Final"
#define MyAppExeName "UI.exe"

[Setup]
AppId={{D593B46D-70C2-44CE-8C42-DB48AB7E84C6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Perfumeria
DefaultGroupName=Perfumeria
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=Output
OutputBaseFilename=Perfumeria_Setup_Iteracion2
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x86compatible x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"; Flags: unchecked
Name: "demodata"; Description: "Cargar actores de demostracion (claves publicas del README; solo pruebas)"; GroupDescription: "Datos de prueba:"; Flags: unchecked

[Files]
Source: "..\UI\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "bin\SeedDemoActors.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "InicializarBaseDatos.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion
Source: "VerificarPrerrequisitos.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion
Source: "VerificarPrerrequisitos.ps1"; Flags: dontcopy
Source: "..\SQL\Trabajo Practico Final 3.bak"; DestDir: "{app}\Installer"; DestName: "TrabajoPracticoFinal.bak"; Flags: ignoreversion
Source: "..\SQL\Iteracion2_Aprovisionamiento.sql"; DestDir: "{app}\Installer\Migraciones"; Flags: ignoreversion
Source: "..\SQL\Iteracion2_Ventas_Fidelizacion.sql"; DestDir: "{app}\Installer\Migraciones"; Flags: ignoreversion

[Icons]
Name: "{group}\Perfumeria"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Perfumeria"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar Perfumeria"; Flags: nowait postinstall skipifsilent

[Code]
var
  SqlServerPage: TInputQueryWizardPage;
  PrerequisitesChecked: Boolean;

procedure InitializeWizard;
begin
  SqlServerPage := CreateInputQueryPage(
    wpSelectDir,
    'Servidor de base de datos',
    'Indique la instancia de SQL Server disponible',
    'El instalador usara autenticacion integrada de Windows y requiere SQL Server 16.x (2022) o superior, incluida la version 17.x. Use . para la instancia predeterminada o .\SQLEXPRESS para Express.');
  SqlServerPage.Add('Instancia SQL Server:', False);
  SqlServerPage.Values[0] := '.';
end;

function GetSqlServerInstance(Param: String): String;
begin
  Result := SqlServerPage.Values[0];
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  Parameters: String;
begin
  if CurStep <> ssPostInstall then
    exit;
  Parameters := '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{app}\Installer\InicializarBaseDatos.ps1') +
    '" -ServerInstance "' + GetSqlServerInstance('') + '" -DatabaseName "Trabajo Practico Final" -BackupPath "' +
    ExpandConstant('{app}\Installer\TrabajoPracticoFinal.bak') + '" -MigrationDirectory "' +
    ExpandConstant('{app}\Installer\Migraciones') + '" -ConfigPath "' + ExpandConstant('{app}\UI.exe.config') +
    '" -LogPath "' + ExpandConstant('{app}\Installer\instalacion-base-datos.log') + '"';
  if WizardIsTaskSelected('demodata') then
    Parameters := Parameters + ' -SeedDemoActors';
  WizardForm.StatusLabel.Caption := 'Preparando la base y verificando las cuentas seleccionadas...';
  if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    RaiseException('No se pudo iniciar la configuracion de la base de datos.');
  if ResultCode <> 0 then
    RaiseException('Fallo la configuracion de la base de datos. No se iniciara la aplicacion. Revise Installer\instalacion-base-datos.log y vuelva a ejecutar el instalador.');
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
  Parameters: String;
  DownloadDirectory: String;
begin
  Result := '';
  if PrerequisitesChecked then
    exit;

  ExtractTemporaryFile('VerificarPrerrequisitos.ps1');
  DownloadDirectory := ExpandConstant('{userappdata}\Perfumeria\Prerrequisitos');
  Parameters := '-NoProfile -ExecutionPolicy Bypass -File "' +
    ExpandConstant('{tmp}\VerificarPrerrequisitos.ps1') + '" -ServerInstance "' +
    GetSqlServerInstance('') + '" -DownloadDirectory "' + DownloadDirectory +
    '" -InstallDotNet -LogPath "' + ExpandConstant('{tmp}\Perfumeria-Prerrequisitos.log') + '"';

  WizardForm.StatusLabel.Caption := 'Verificando .NET Framework y SQL Server 16.x o superior...';
  if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Parameters, '', SW_HIDE,
    ewWaitUntilTerminated, ResultCode) then
  begin
    Result := 'No se pudo ejecutar la verificacion de prerrequisitos.';
    exit;
  end;

  case ResultCode of
    0:
      begin
        PrerequisitesChecked := True;
        Result := '';
      end;
    20:
      Result := 'No se encontro la instancia indicada. Se descargo el instalador oficial de SQL Server 2022 Express en: ' +
        DownloadDirectory + '. Instalelo y vuelva a ejecutar este instalador.';
    21:
      begin
        ShellExec('', 'https://www.microsoft.com/en-us/download/details.aspx?id=104781', '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
        Result := 'No se pudo conectar a la instancia indicada y Microsoft no entrego el instalador automatico. Revise el nombre, el servicio y los permisos de SQL Server. Si falta el motor, instale SQL Server 2022 o superior desde la pagina oficial y vuelva a ejecutar este instalador.';
      end;
    22:
      Result := 'La instancia seleccionada es anterior a SQL Server 16.x (2022). Indique una instancia 16.x o superior (por ejemplo, 17.x).';
    30:
      begin
        NeedsRestart := True;
        Result := '.NET Framework fue instalado y Windows debe reiniciarse antes de continuar.';
      end;
    else
      Result := 'Fallo la verificacion de prerrequisitos. Revise el registro temporal Perfumeria-Prerrequisitos.log.';
  end;
end;
