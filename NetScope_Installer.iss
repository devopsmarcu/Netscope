; Script de Instalação Inno Setup - NetScope
; Versão Atualizada: 2.1 (Genérica)
; Desenvolvido para Marcus Santos

#define AppName "NetScope"
#define AppVersion "2.1"
#define AppPublisher "Marcus Santos"
#define AppURL "https://github.com/devopsmarcu/Netscope"
#define AppExeName "NetScope.exe"
#define AppIconName "ico-f.ico"
#define AppId "{{A9D8E3B4-F2C1-4D5E-8A3B-B0A2C3D4E5F6}"

[Setup]
; AppId único para o aplicativo
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes

; Arquivos de imagem e licença (Caminhos relativos ao arquivo .iss)
LicenseFile=license.txt
SetupIconFile={#AppIconName}

; Configurações do Instalador
OutputDir=Installer
OutputBaseFilename=NetScope_v{#AppVersion}_Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; Requisitos de Sistema
MinVersion=10.0
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

; Desinstalação
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName} (Ferramenta de Gerenciamento DHCP)

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Origem dos arquivos (Pasta de Publish gerada pelo Visual Studio / dotnet publish)
Source: "bin\Release\net9.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Script PowerShell para verificação de políticas
Source: "CheckMacPolicy.ps1"; DestDir: "{app}"; Flags: ignoreversion

; Ícone do sistema
Source: "ico-f.ico"; DestDir: "{app}"; Flags: ignoreversion

; OBS: O arquivo database.json é criado automaticamente pela aplicação na primeira execução.
; Se desejar incluir um banco pré-configurado, adicione a linha abaixo:
; Source: "database.json"; DestDir: "{app}"; Flags: onlyifdoesntexist

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Verificações customizadas podem ser adicionadas aqui no futuro
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Log('Instalação do NetScope ' + '{#AppVersion}' + ' concluída.');
  end;
end;
