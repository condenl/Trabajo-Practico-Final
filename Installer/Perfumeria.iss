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
Name: "demodata"; Description: "Cargar actores y escenarios demo (claves publicas del README; solo pruebas)"; GroupDescription: "Datos de prueba:"; Flags: unchecked

[Files]
Source: "..\UI\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "bin\SeedDemoActors.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Documentacion\Licencias_Reportes.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Documentacion\Ejemplos\ordenes-compra-3.xml"; DestDir: "{app}\Documentacion\Ejemplos"; Flags: ignoreversion
Source: "InicializarBaseDatos.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion
Source: "VerificarPrerrequisitos.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion
Source: "VerificarPrerrequisitos.ps1"; Flags: dontcopy
Source: "..\SQL\Trabajo Practico Final 3.bak"; DestDir: "{app}\Installer"; DestName: "TrabajoPracticoFinal.bak"; Flags: ignoreversion
Source: "..\SQL\Iteracion2_Aprovisionamiento.sql"; DestDir: "{app}\Installer\Migraciones"; Flags: ignoreversion
Source: "..\SQL\Iteracion2_Ventas_Fidelizacion.sql"; DestDir: "{app}\Installer\Migraciones"; Flags: ignoreversion
Source: "..\SQL\Iteracion3_Evaluacion_Proveedores.sql"; DestDir: "{app}\Installer\Migraciones"; Flags: ignoreversion
Source: "..\SQL\Iteracion3_Pulido_Demo.sql"; DestDir: "{app}\Installer\Migraciones"; Flags: ignoreversion

[Icons]
Name: "{group}\Perfumeria"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Perfumeria"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar Perfumeria"; Flags: nowait postinstall skipifsilent

[Code]
var
  SqlServerPage: TInputQueryWizardPage;
  SqlStatusLabel: TNewStaticText;
  SqlDownloadButton: TNewButton;
  SqlServicesButton: TNewButton;
  SqlDiscoveryAttempted: Boolean;

procedure DownloadSqlClick(Sender: TObject);
var
  ResultCode: Integer;
begin
  if not ShellExec('open', 'https://www.microsoft.com/es-es/sql-server/sql-server-downloads',
    '', '', SW_SHOWNORMAL, ewNoWait, ResultCode) then
    MsgBox('No se pudo abrir el navegador. Visite https://www.microsoft.com/es-es/sql-server/sql-server-downloads y descargue SQL Server Express 2022 o superior.', mbInformation, MB_OK);
end;

procedure OpenSqlServicesClick(Sender: TObject);
var
  ResultCode: Integer;
begin
  if not ShellExec('open', ExpandConstant('{sys}\services.msc'), '', '', SW_SHOWNORMAL, ewNoWait, ResultCode) then
    MsgBox('Abra Servicios desde el menú Inicio de Windows. Busque SQL Server y active el servicio de la instancia elegida.', mbInformation, MB_OK);
end;

procedure UpdateSqlActions(Code: Integer);
begin
  // Offer a download only when the engine is absent or the selected version is too old.
  SqlDownloadButton.Visible := (Code = 20) or (Code = 22);
  SqlServicesButton.Visible := Code = 24;
end;

procedure SqlInstanceChanged(Sender: TObject);
begin
  SqlStatusLabel.Font.Color := clWindowText;
  SqlStatusLabel.Caption := 'Pulse Siguiente para comprobar la instancia elegida.';
  UpdateSqlActions(0);
end;

procedure InitializeWizard;
begin
  SqlServerPage := CreateInputQueryPage(
    wpSelectDir,
    'Servidor de base de datos',
    'Seleccione una instancia para guardar los datos de Perfumería',
    'Requiere SQL Server 2022 (16.x) o superior y autenticación de Windows. Use . para la instancia predeterminada, .\SQLEXPRESS para Express o (localdb)\MSSQLLocalDB para LocalDB.');
  SqlServerPage.Add('Instancia SQL Server:', False);
  SqlServerPage.Values[0] := ExpandConstant('{param:SQLSERVER|}');

  SqlDownloadButton := TNewButton.Create(WizardForm);
  SqlDownloadButton.Parent := SqlServerPage.Surface;
  SqlDownloadButton.Left := 0;
  SqlDownloadButton.Top := SqlServerPage.Edits[0].Top + SqlServerPage.Edits[0].Height + ScaleY(12);
  SqlDownloadButton.Width := ScaleX(190);
  SqlDownloadButton.Height := ScaleY(25);
  SqlDownloadButton.Caption := 'Descargar SQL Server';
  SqlDownloadButton.OnClick := @DownloadSqlClick;
  SqlDownloadButton.Visible := False;

  SqlServicesButton := TNewButton.Create(WizardForm);
  SqlServicesButton.Parent := SqlServerPage.Surface;
  SqlServicesButton.Left := SqlDownloadButton.Left;
  SqlServicesButton.Top := SqlDownloadButton.Top;
  SqlServicesButton.Width := ScaleX(140);
  SqlServicesButton.Height := SqlDownloadButton.Height;
  SqlServicesButton.Caption := 'Abrir servicios';
  SqlServicesButton.OnClick := @OpenSqlServicesClick;
  SqlServicesButton.Visible := False;

  SqlStatusLabel := TNewStaticText.Create(WizardForm);
  SqlStatusLabel.Parent := SqlServerPage.Surface;
  SqlStatusLabel.Left := 0;
  SqlStatusLabel.Top := SqlDownloadButton.Top + SqlDownloadButton.Height + ScaleY(10);
  SqlStatusLabel.Width := SqlServerPage.SurfaceWidth;
  SqlStatusLabel.Height := SqlServerPage.SurfaceHeight - SqlStatusLabel.Top;
  SqlStatusLabel.AutoSize := False;
  SqlStatusLabel.WordWrap := True;
  SqlStatusLabel.Caption := 'Indique una instancia y pulse Siguiente para comprobar la conexión y la versión de SQL Server.';
  SqlServerPage.Edits[0].OnChange := @SqlInstanceChanged;
end;

procedure CurPageChanged(CurPageID: Integer);
var
  Parameters: String;
  ResultFile: String;
  SelectedInstance: String;
  ResultCode: Integer;
begin
  if (CurPageID <> SqlServerPage.ID) or SqlDiscoveryAttempted then exit;
  SqlDiscoveryAttempted := True;
  // Respect /SQLSERVER and any value already entered; never replace a user's choice.
  if Trim(SqlServerPage.Values[0]) <> '' then exit;
  SqlStatusLabel.Caption := 'Buscando una instancia local compatible. Esto puede tardar unos segundos...';
  SqlStatusLabel.Repaint;
  WizardForm.NextButton.Enabled := False;
  try
    ExtractTemporaryFile('VerificarPrerrequisitos.ps1');
    ResultFile := ExpandConstant('{tmp}\Perfumeria-Instancias.ini');
    DeleteFile(ResultFile);
    Parameters := '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}\VerificarPrerrequisitos.ps1') +
      '" -DiscoverSqlInstances -ResultPath "' + ResultFile + '" -LogPath "' +
      ExpandConstant('{tmp}\Perfumeria-Instancias.log') + '"';
    if Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Parameters, '', SW_HIDE,
      ewWaitUntilTerminated, ResultCode) then
    begin
      SelectedInstance := GetIniString('Prerequisites', 'SelectedInstance', '', ResultFile);
      if (ResultCode = 0) and (Trim(SqlServerPage.Values[0]) = '') then
        SqlServerPage.Values[0] := SelectedInstance;
      SqlStatusLabel.Caption := GetIniString('Prerequisites', 'Message',
        'No se pudo elegir una instancia automáticamente. Ingrese su nombre y pulse Siguiente.', ResultFile);
      UpdateSqlActions(ResultCode);
    end
    else
      SqlStatusLabel.Caption := 'No se pudo buscar una instancia automáticamente. Ingrese su nombre y pulse Siguiente.';
  finally
    WizardForm.NextButton.Enabled := True;
  end;
end;

function GetSqlServerInstance(Param: String): String;
begin
  Result := Trim(SqlServerPage.Values[0]);
end;

function ValidSqlInstance: Boolean;
var
  Instance: String;
  I: Integer;
begin
  Instance := GetSqlServerInstance('');
  Result := (Length(Instance) > 0) and (Length(Instance) <= 128);
  for I := 1 to Length(Instance) do
    if Pos(Instance[I], 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.\()-') = 0 then
      Result := False;
end;

function CheckPrerequisites(SqlOnly: Boolean; var Message: String): Integer;
var
  Parameters: String;
  ResultFile: String;
begin
  Result := 26;
  Message := 'Ingrese un nombre de instancia válido, por ejemplo ., .\SQLEXPRESS o (localdb)\MSSQLLocalDB.';
  if not ValidSqlInstance then exit;
  ExtractTemporaryFile('VerificarPrerrequisitos.ps1');
  ResultFile := ExpandConstant('{tmp}\Perfumeria-Prerrequisitos.ini');
  DeleteFile(ResultFile);
  Parameters := '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}\VerificarPrerrequisitos.ps1') +
    '" -ServerInstance "' + GetSqlServerInstance('') + '" -DownloadDirectory "' +
    ExpandConstant('{userappdata}\Perfumeria\Prerrequisitos') + '" -ResultPath "' + ResultFile +
    '" -LogPath "' + ExpandConstant('{tmp}\Perfumeria-Prerrequisitos.log') + '"';
  if SqlOnly then Parameters := Parameters + ' -CheckSqlOnly'
  else Parameters := Parameters + ' -InstallDotNet';
  if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Parameters, '', SW_HIDE,
    ewWaitUntilTerminated, Result) then
  begin
    Result := 99;
    Message := 'No se pudo iniciar la verificación. Compruebe que Windows PowerShell esté disponible.';
    exit;
  end;
  Message := GetIniString('Prerequisites', 'Message',
    'No se pudo completar la verificación. Revise el registro temporal Perfumeria-Prerrequisitos.log.', ResultFile);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Code: Integer;
  Message: String;
begin
  Result := True;
  if CurPageID <> SqlServerPage.ID then exit;
  SqlStatusLabel.Font.Color := clWindowText;
  SqlStatusLabel.Caption := 'Comprobando la conexión y el estado de SQL Server...';
  UpdateSqlActions(0);
  SqlStatusLabel.Repaint;
  WizardForm.NextButton.Enabled := False;
  try
    Code := CheckPrerequisites(True, Message);
  finally
    WizardForm.NextButton.Enabled := True;
  end;
  SqlStatusLabel.Caption := Message;
  UpdateSqlActions(Code);
  Result := Code = 0;
  if not Result then SqlStatusLabel.Font.Color := clRed;
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
  Message: String;
begin
  Result := '';
  WizardForm.StatusLabel.Caption := 'Verificando .NET Framework y la conexión con SQL Server...';
  // Always recheck: the chosen instance or its service may have changed since the SQL page.
  ResultCode := CheckPrerequisites(False, Message);
  UpdateSqlActions(ResultCode);
  if ResultCode = 30 then NeedsRestart := True;
  if ResultCode <> 0 then
    Result := Message + #13#10#13#10 + 'Vuelva a la página «Servidor de base de datos» para corregir la instancia, abrir Servicios o descargar el motor desde Microsoft. Luego vuelva a intentar.';
end;
