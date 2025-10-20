#define MyAppName "iWebIT Print Agent"
#define MyAppExeName "iWebIT_PrintAgent.exe"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "iWebIT"
#define MyAppDirName "iWebIT_PrintAgent"
#define MyAppDir "{pf}\iWebIT_PrintAgent"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={#MyAppDir}
DefaultGroupName={#MyAppName}
OutputBaseFilename=iWebIT_PrintAgent_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
;SetupIconFile=icon.ico
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\iWebIT_PrintAgent\bin\Release\net8.0-windows\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Run]
; Criar o serviço
Filename: "sc.exe"; Parameters: "create iWebIT_PrintAgent binPath= ""{app}\{#MyAppExeName}"" start= auto"; Flags: runhidden waituntilterminated
; Adicionar descrição
Filename: "sc.exe"; Parameters: "description iWebIT_PrintAgent ""{#MyAppName} - imprime trabalhos automaticamente"""; Flags: runhidden waituntilterminated
; Iniciar serviço
Filename: "sc.exe"; Parameters: "start iWebIT_PrintAgent"; Flags: runhidden waituntilterminated

[UninstallRun]
; Parar e remover serviço
Filename: "sc.exe"; Parameters: "stop iWebIT_PrintAgent"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "delete iWebIT_PrintAgent"; Flags: runhidden waituntilterminated
