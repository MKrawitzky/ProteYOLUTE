; ProteYOLUTE Installer Script
; Inno Setup 6.x
; Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.

#define MyAppName "ProteYOLUTE"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Michael Krawitzky"
#define MyAppURL "https://github.com/MKrawitzky/ProteYOLUTE"

; Root of the project (one level up from this .iss file)
#define ProjectRoot ".."

; Default HyStar plugin install path
#define DefaultInstallDir "C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute"

; HyStar proteoElute program directory (where DLLs are deployed)
#define HyStarDir "C:\Program Files (x86)\Bruker Daltonik\HyStar\proteoElute"

[Setup]
AppId={{B7E4F3A1-9C2D-4E8B-A1F6-3D7C5E9B2A4F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={#DefaultInstallDir}
DefaultGroupName={#MyAppName}
LicenseFile={#ProjectRoot}\LICENSE
OutputDir=Output
OutputBaseFilename=ProteYOLUTE_Setup_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
DisableProgramGroupPage=yes
PrivilegesRequired=admin
SetupIconFile={#ProjectRoot}\Installer\proteyolute.ico
UninstallDisplayIcon={app}\proteyolute.ico

; Warn user that HyStar should be closed
CloseApplications=force
CloseApplicationsFilter=HyStar.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nProteYOLUTE v2.0 — Intelligent Nano-Flow LC Platform%n%nIncludes: SQLite data engine, smart column/trap monitoring, pressure anomaly detection, REST API + web dashboard, animated flow visualization, 3-tier error recovery, and configurable topology.%n%nIMPORTANT: Close HyStar before proceeding.

[Types]
Name: "full"; Description: "Full installation (Plugin + DLLs + Tools + Dashboard)"
Name: "plugin"; Description: "Plugin only (Lua scripts + DLLs)"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Components]
Name: "core"; Description: "Core Plugin (Lua scripts + topology config)"; Types: full plugin custom; Flags: fixed
Name: "dlls"; Description: "Plugin DLLs (BalticWpfControlLib + BrukerLC.Styling + SQLite)"; Types: full plugin custom; Flags: fixed
Name: "images"; Description: "System Diagram Images"; Types: full plugin custom
Name: "docs"; Description: "Documentation (README, Roadmap, Rewrite Report)"; Types: full custom
Name: "tools"; Description: "Standalone Tools"; Types: full custom
Name: "tools\diagrameditor"; Description: "ProteYOLUTE Diagram Editor"; Types: full custom
Name: "tools\valveeditor"; Description: "ProteYOLUTE Valve Editor"; Types: full custom

; ============================================================
; FILES
; ============================================================
[Files]
; --- Lua scripts (root level) ---
Source: "{#ProjectRoot}\*.lua"; DestDir: "{app}"; Components: core; Flags: ignoreversion

; --- Packages (Lua libraries — includes new smart_data, error_recovery, topology, proteyolute_core) ---
Source: "{#ProjectRoot}\Packages\*.lua"; DestDir: "{app}\Packages"; Components: core; Flags: ignoreversion

; --- Topology configuration ---
Source: "{#ProjectRoot}\topology.json"; DestDir: "{app}"; Components: core; Flags: ignoreversion

; --- XML configuration files (preserve user settings) ---
Source: "{#ProjectRoot}\Preferences.xml"; DestDir: "{app}"; Components: core; Flags: onlyifdoesntexist uninsneveruninstall
Source: "{#ProjectRoot}\PumpSettings.xml"; DestDir: "{app}"; Components: core; Flags: onlyifdoesntexist uninsneveruninstall
Source: "{#ProjectRoot}\PumpLimits.xml"; DestDir: "{app}"; Components: core; Flags: onlyifdoesntexist uninsneveruninstall
Source: "{#ProjectRoot}\InstalledComponents.xml"; DestDir: "{app}"; Components: core; Flags: onlyifdoesntexist uninsneveruninstall
Source: "{#ProjectRoot}\ServiceAccess.xml"; DestDir: "{app}"; Components: core; Flags: onlyifdoesntexist uninsneveruninstall
Source: "{#ProjectRoot}\ColumnSelections.xml"; DestDir: "{app}"; Components: core; Flags: onlyifdoesntexist uninsneveruninstall

; --- System diagram images ---
Source: "{#ProjectRoot}\Images\*"; DestDir: "{app}\Images"; Components: images; Flags: ignoreversion

; --- BalticWpfControlLib.dll → deploy to HyStar proteoElute folder ---
Source: "{#ProjectRoot}\DLL_Extract\dnspy_baltic\BalticWpfControlLib\bin\Release\BalticWpfControlLib.dll"; DestDir: "{#HyStarDir}"; Components: dlls; Flags: ignoreversion

; --- SQLite dependencies → deploy to HyStar proteoElute folder ---
Source: "{#ProjectRoot}\DLL_Extract\dnspy_baltic\packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.0\lib\net46\System.Data.SQLite.dll"; DestDir: "{#HyStarDir}"; Components: dlls; Flags: ignoreversion
Source: "{#ProjectRoot}\DLL_Extract\dnspy_baltic\packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.0\build\net46\x64\SQLite.Interop.dll"; DestDir: "{#HyStarDir}\x64"; Components: dlls; Flags: ignoreversion

; --- Web dashboard HTML → deploy to HyStar proteoElute folder ---
Source: "{#ProjectRoot}\DLL_Extract\dnspy_baltic\BalticWpfControlLib\Api\dashboard.html"; DestDir: "{#HyStarDir}\Api"; Components: dlls; Flags: ignoreversion

; --- BrukerLC.Styling DLL → deploy to HyStar proteoElute folder ---
Source: "{#ProjectRoot}\DLL_Extract\dnspy_styling\BrukerLC.Styling\bin\Release\BrukerLC.Styling.dll"; DestDir: "{#HyStarDir}"; Components: dlls; Flags: ignoreversion

; --- Keep build artifacts in project for development ---
Source: "{#ProjectRoot}\DLL_Extract\dnspy_baltic\BalticWpfControlLib\bin\Release\*.dll"; DestDir: "{app}\DLL_Extract\dnspy_baltic\BalticWpfControlLib\bin\Release"; Components: dlls; Flags: ignoreversion
Source: "{#ProjectRoot}\DLL_Extract\dnspy_styling\BrukerLC.Styling\bin\Release\*.dll"; DestDir: "{app}\DLL_Extract\dnspy_styling\BrukerLC.Styling\bin\Release"; Components: dlls; Flags: ignoreversion

; --- Diagram Editor ---
Source: "{#ProjectRoot}\Tools\DiagramEditor\bin\Release\net48\ProteYOLUTE.DiagramEditor.exe"; DestDir: "{app}\Tools\DiagramEditor"; Components: tools\diagrameditor; Flags: ignoreversion
Source: "{#ProjectRoot}\Tools\DiagramEditor\bin\Release\net48\ProteYOLUTE.DiagramEditor.exe.config"; DestDir: "{app}\Tools\DiagramEditor"; Components: tools\diagrameditor; Flags: ignoreversion

; --- Valve Editor ---
Source: "{#ProjectRoot}\Tools\ValveEditor\bin\Publish\ProteYOLUTE.ValveEditor.exe"; DestDir: "{app}\Tools\ValveEditor"; Components: tools\valveeditor; Flags: ignoreversion
Source: "{#ProjectRoot}\Tools\ValveEditor\bin\Publish\ProteYOLUTE.ValveEditor.exe.config"; DestDir: "{app}\Tools\ValveEditor"; Components: tools\valveeditor; Flags: ignoreversion

; --- Documentation ---
Source: "{#ProjectRoot}\README.md"; DestDir: "{app}"; Components: docs; Flags: ignoreversion
Source: "{#ProjectRoot}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\ROADMAP.md"; DestDir: "{app}"; Components: docs; Flags: ignoreversion
Source: "{#ProjectRoot}\REWRITE_REPORT.md"; DestDir: "{app}"; Components: docs; Flags: ignoreversion
Source: "{#ProjectRoot}\REWRITE_REPORT.pdf"; DestDir: "{app}"; Components: docs; Flags: ignoreversion

; --- Icon ---
Source: "{#ProjectRoot}\Installer\proteyolute.ico"; DestDir: "{app}"; Flags: ignoreversion

; ============================================================
; BACKUP ORIGINALS BEFORE INSTALL
; ============================================================
[InstallDelete]
; Don't delete anything — we back up in [Code] section

; ============================================================
; SHORTCUTS
; ============================================================
[Icons]
Name: "{group}\ProteYOLUTE Diagram Editor"; Filename: "{app}\Tools\DiagramEditor\ProteYOLUTE.DiagramEditor.exe"; Components: tools\diagrameditor
Name: "{group}\ProteYOLUTE Valve Editor"; Filename: "{app}\Tools\ValveEditor\ProteYOLUTE.ValveEditor.exe"; Components: tools\valveeditor
Name: "{group}\ProteYOLUTE Dashboard"; Filename: "http://localhost:8742"; IconFilename: "{app}\proteyolute.ico"
Name: "{group}\Uninstall ProteYOLUTE"; Filename: "{uninstallexe}"
Name: "{commondesktop}\ProteYOLUTE Diagram Editor"; Filename: "{app}\Tools\DiagramEditor\ProteYOLUTE.DiagramEditor.exe"; Components: tools\diagrameditor; Tasks: desktopicons
Name: "{commondesktop}\ProteYOLUTE Valve Editor"; Filename: "{app}\Tools\ValveEditor\ProteYOLUTE.ValveEditor.exe"; Components: tools\valveeditor; Tasks: desktopicons

[Tasks]
Name: "desktopicons"; Description: "Create desktop shortcuts for Tools"; Components: tools
Name: "backupdlls"; Description: "Backup original Bruker DLLs before overwriting"; Flags: checkedonce

; ============================================================
; .NET FRAMEWORK 4.8 CHECK + DLL BACKUP
; ============================================================
[Code]
function IsDotNetInstalled(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release);
  if Result then
    Result := (Release >= 528040);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsDotNetInstalled() then
  begin
    MsgBox('ProteYOLUTE requires .NET Framework 4.8 or later.' + #13#10 + #13#10 +
           'Please install .NET Framework 4.8 from Microsoft and run this installer again.',
           mbCriticalError, MB_OK);
    Result := False;
  end;
end;

procedure BackupDLL(const DLLName: String);
var
  SrcPath, BackupPath: String;
begin
  SrcPath := ExpandConstant('{#HyStarDir}\') + DLLName;
  BackupPath := SrcPath + '.ORIGINAL';
  if FileExists(SrcPath) and not FileExists(BackupPath) then
  begin
    FileCopy(SrcPath, BackupPath, True);
    Log('Backed up ' + SrcPath + ' to ' + BackupPath);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    if IsTaskSelected('backupdlls') then
    begin
      BackupDLL('BalticWpfControlLib.dll');
      BackupDLL('BrukerLC.Styling.dll');
    end;
  end;
end;

[Run]
Filename: "{app}\Tools\DiagramEditor\ProteYOLUTE.DiagramEditor.exe"; Description: "Launch Diagram Editor"; Flags: nowait postinstall skipifsilent unchecked; Components: tools\diagrameditor
