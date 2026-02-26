; Script de Instalação Inno Setup - NetScope
; Versão Atualizada: 1.0
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

; Seleção de Idioma
ShowLanguageDialog=yes
UsePreviousLanguage=no

; Arquivos de imagem e licença (Caminhos relativos ao arquivo .iss)
LicenseFile=LICENSE
SetupIconFile={#AppIconName}

; Configurações do Instalador
OutputDir=Resources
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
UninstallDisplayName={cm:UninstallDisplay}

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]
brazilianportuguese.UninstallDisplay={#AppName} (Ferramenta de Gerenciamento DHCP)
english.UninstallDisplay={#AppName} (DHCP Management Tool)
spanish.UninstallDisplay={#AppName} (Herramienta de Gestión DHCP)

brazilianportuguese.InstallComplete=Instalação do NetScope {#AppVersion} concluída.
english.InstallComplete=NetScope {#AppVersion} installation completed.
spanish.InstallComplete=Instalación de NetScope {#AppVersion} completada.

brazilianportuguese.LanguageCode=pt-BR
english.LanguageCode=en-US
spanish.LanguageCode=es-ES

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Origem dos arquivos (Pasta de Publish gerada pelo Visual Studio / dotnet publish)
Source: "bin\Release\net9.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; OBS: O arquivo database.json é criado automaticamente pela aplicação na primeira execução.

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Procedimento para salvar a preferência de idioma no arquivo de configuração da aplicação
procedure SaveLanguagePreference;
var
  JsonContent: string;
  DataDir: string;
  DataFile: string;
  LangCode: string;
begin
  LangCode := CustomMessage('LanguageCode');
  DataDir := ExpandConstant('{userappdata}\{#AppName}');
  DataFile := DataDir + '\database.json';

  if not DirExists(DataDir) then
    ForceDirectories(DataDir);

  // Se o arquivo não existe, cria um básico com o idioma selecionado
  if not FileExists(DataFile) then
  begin
    JsonContent := '{' + #13#10 +
                   '  "Servers": [],' + #13#10 +
                   '  "Policies": [],' + #13#10 +
                   '  "Scopes": [],' + #13#10 +
                   '  "Settings": {' + #13#10 +
                   '    "Language": "' + LangCode + '"' + #13#10 +
                   '  }' + #13#10 +
                   '}';
    SaveStringToFile(DataFile, JsonContent, False);
    Log('Arquivo de configuração criado com idioma: ' + LangCode);
  end
  else
  begin
    // Se o arquivo já existe, poderíamos atualizar o campo "Language".
    // Para simplificar e garantir a escalabilidade pretendida, registramos no log.
    // A aplicação também permite mudar o idioma internamente.
    Log('Arquivo de configuração já existe. O idioma selecionado foi: ' + LangCode);
    
    // Opcional: Sobrescrever apenas a configuração de idioma (necessário parser JSON ou substituição manual)
    // Por enquanto, criamos apenas se for nova instalação, conforme padrão de persistência.
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    SaveLanguagePreference;
    Log(CustomMessage('InstallComplete'));
  end;
end;
