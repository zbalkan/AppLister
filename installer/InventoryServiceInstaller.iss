; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppShortName "InventorySvc"
#define MyAppDisplayName "Inventory Service"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Zafer Balkan"
#define MyAppURL "https://github.com/zbalkan/InventoryService"
#define ParentKey "Software\zb"
#define Copyright "Zafer Balkan 2023"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{70DED3F6-40F9-471C-8EC7-7EB28349632E}
AppName={#MyAppDisplayName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppShortName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright={#Copyright}

DefaultDirName={commonpf}\{#MyAppDisplayName}
DefaultGroupName={#MyAppShortName}
DisableProgramGroupPage=yes
OutputDir=.\bin
OutputBaseFilename="{#MyAppDisplayName} Installer"
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
WizardStyle=modern
Uninstallable=yes
AlwaysShowDirOnReadyPage=false


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\src\WindowsServiceProxy\bin\x64\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
;recursesubdirs createallsubdirs
Source: "..\src\WindowsServiceProxy\bin\x64\Release\*.exe"; DestDir: "{app}"; Flags: ignoreversion
;recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKLM; Subkey: {#ParentKey}; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: {#ParentKey}\{#MyAppShortName}; Flags: uninsdeletekey
Root: HKLM; Subkey: {#ParentKey}\{#MyAppShortName}; ValueType: dword; ValueName: "QueryPeriodInMinutes"; ValueData: 10

[Run]
; Register WMI class
Filename: {dotnet40}\InstallUtil.exe; Parameters: """{app}\InventoryWmiProvider.dll"" /LogFile=""{tmp}"" /ShowCallStack" ; Flags: runhidden runascurrentuser;
; Register and start service
Filename: {sys}\sc.exe; Parameters: "create ""{#MyAppShortName}"" start= auto binPath= ""{app}\WindowsServiceProxy.exe"" displayname=""{#MyAppDisplayName}""" ; Flags: runhidden runascurrentuser;
Filename: {sys}\sc.exe; Parameters: "description ""{#MyAppShortName}"" ""Creates software inventory and publishes as WMI object instances.""" ; Flags: runhidden runascurrentuser;
Filename: {sys}\sc.exe; Parameters: "failure ""{#MyAppShortName}"" actions= restart/60000/restart/60000/""""/60000 reset= 86400" ; Flags: runhidden runascurrentuser;
Filename: {sys}\sc.exe; Parameters: "start ""{#MyAppShortName}""" ; Flags: runhidden runascurrentuser; Description: "Start service";

[UninstallRun]
; Srop and delete service
Filename: {sys}\sc.exe; Parameters: "stop ""{#MyAppShortName}""" ; Flags: runhidden runascurrentuser; RunOnceId: "StopService";
Filename: {sys}\sc.exe; Parameters: "delete ""{#MyAppShortName}""" ; Flags: runhidden runascurrentuser; RunOnceId: "DeleteService";
; Unregister WMI class
Filename: {dotnet40}\InstallUtil.exe; Parameters: "/u ""{app}\InventoryWmiProvider.dll"" /LogFile=""{tmp}"" /ShowCallStack" ; Flags: runhidden runascurrentuser; RunOnceId: "DelWmiNamespace";
