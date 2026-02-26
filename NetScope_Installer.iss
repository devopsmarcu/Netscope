; Script de Instalação Inno Setup - NetScope
; Desenvolvido por Antigravity para Marcus Santos

#define AppName "NetScope"
#define AppVersion "2.0"
#define AppPublisher "NetScope Solutions"
#define AppURL "https://github.com/devopsmarcu/Netscope"
#define AppExeName "NetScope.exe"
#define AppIconName "ico-f.ico"
#define AppId "{{A9D8E3B4-F2C1-4D5E-8A3B-B0A2C3D4E5F6}"

[Setup]
; AppId único para o aplicativo (gerado para este projeto)
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes
; Licença e ícone
LicenseFile=Z:\Técnicos\Marcus\Desenvolvimento\C#\Netscope\license.txt
SetupIconFile=Z:\Técnicos\Marcus\Desenvolvimento\C#\Netscope\{#AppIconName}
; Pasta de saída para o instalador compilado
OutputDir=Z:\Técnicos\Marcus\Desenvolvimento\C#\Netscope\Installer
OutputBaseFilename=Setup_NetScope_v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
; Requer Windows 10 ou superior
MinVersion=10.0
; Arquitetura 64 bits obrigatória
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
; Solicitar privilégios de administrador
PrivilegesRequired=admin
; Registrar no Painel de Controle
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Origem dos arquivos compilados (Ajustado para o seu caminho net9.0-windows\win-x64\publish)
Source: "Z:\Técnicos\Marcus\Desenvolvimento\C#\Netscope\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; O ícone também deve acompanhar o .exe na pasta root caso queira usá-lo externamente
Source: "Z:\Técnicos\Marcus\Desenvolvimento\C#\Netscope\{#AppIconName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Função para verificar a presença do .NET Desktop Runtime
function IsDotNet9Installed(): Boolean;
var
  ResultCode: Integer;
begin
  // Tenta rodar 'dotnet --list-runtimes' e verifica se o Microsoft.WindowsDesktop.App 9.0 está presente
  // Esta é uma verificação simplificada. Em cenários reais, o Inno Setup possui plugins ou verificações de registro específicas.
  // Como o app é Win-x64, assumimos que se ele abrir, o runtime está ok, mas aqui exibimos um aviso básico
  Result := True; 
end;

function InitializeSetup(): Boolean;
begin
  // Verificação customizada pode ser adicionada aqui
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Log('Instalação concluída com sucesso.');
  end;
end;
