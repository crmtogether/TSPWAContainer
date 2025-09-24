; CRMTogether PWA Host Installer Script - Sage100 Edition
; Generated for CRMTogether.PwaHost.Sage100 v2.0.0.0

#define MyAppName "CRM Together App Bridge - Sage100"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "CRMTogether"
#define MyAppURL "https://appmxs100.crmtogether.com"
#define MyAppExeName "CRMTogether.PwaHost.Sage100.exe"
#define MyAppDescription "WinForms WebView2 host for Sage100 integration with clipboard monitoring, custom URI handling, and multi-language support"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{B2C3D4E5-F6G7-8901-BCDE-F23456789012}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userappdata}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=
InfoAfterFile=
OutputDir=installer
OutputBaseFilename=SetupContextAI_Sage100_Client
SetupIconFile=images\crmtogethericon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64 arm64
ArchitecturesInstallIn64BitMode=x64 arm64
MinVersion=6.1sp1
DisableProgramGroupPage=yes
DisableReadyPage=no
DisableFinishedPage=no
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppDescription}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "associateeml"; Description: "Associate .eml files with {#MyAppName}"; GroupDescription: "File associations:"; Flags: checkedonce
Name: "associatephone"; Description: "Associate .phone files with {#MyAppName}"; GroupDescription: "File associations:"; Flags: checkedonce
Name: "registerprotocol"; Description: "Register crmtog-sage100:// protocol handler"; GroupDescription: "Protocol handlers:"; Flags: checkedonce

[Files]
; Main application executable
Source: "bin\x64\Release\net481\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\{#MyAppExeName}.pdb"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; Application icon
Source: "bin\x64\Release\net481\images\crmtogethericon.ico"; DestDir: "{app}\images"; Flags: ignoreversion

; Translation files
Source: "bin\x64\Release\net481\translations\*.json"; DestDir: "{app}\translations"; Flags: ignoreversion

; Environment configuration files
Source: "bin\x64\Release\net481\config\*.json"; DestDir: "{app}\config"; Flags: ignoreversion

; .NET Framework dependencies
Source: "bin\x64\Release\net481\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\Microsoft.Web.WebView2.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\Microsoft.Web.WebView2.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\Microsoft.Web.WebView2.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\MimeKit.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.IO.Pipelines.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Text.Encodings.Web.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Text.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion

; WebView2 runtime files
Source: "bin\x64\Release\net481\runtimes\win-x64\native\WebView2Loader.dll"; DestDir: "{app}\runtimes\win-x64\native"; Flags: ignoreversion
Source: "bin\x64\Release\net481\runtimes\win-x86\native\WebView2Loader.dll"; DestDir: "{app}\runtimes\win-x86\native"; Flags: ignoreversion skipifsourcedoesntexist
Source: "bin\x64\Release\net481\runtimes\win-arm64\native\WebView2Loader.dll"; DestDir: "{app}\runtimes\win-arm64\native"; Flags: ignoreversion skipifsourcedoesntexist

; Additional dependencies
Source: "bin\x64\Release\net481\BouncyCastle.Cryptography.dll"; DestDir: "{app}"; Flags: ignoreversion

; Documentation
Source: "README.txt"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\images\crmtogethericon.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\images\crmtogethericon.ico"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\images\crmtogethericon.ico"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; Register .eml file association for Sage100
Root: HKCR; Subkey: ".eml"; ValueType: string; ValueName: ""; ValueData: "CRMTogetherEMLSage100"; Flags: uninsdeletevalue; Tasks: associateeml
Root: HKCR; Subkey: "CRMTogetherEMLSage100"; ValueType: string; ValueName: ""; ValueData: "CRM Together EML File - Sage100"; Flags: uninsdeletekey; Tasks: associateeml
Root: HKCR; Subkey: "CRMTogetherEMLSage100\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: associateeml
Root: HKCR; Subkey: "CRMTogetherEMLSage100\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associateeml

; Register .phone file association for Sage100
Root: HKCR; Subkey: ".phone"; ValueType: string; ValueName: ""; ValueData: "CRMTogetherPhoneSage100"; Flags: uninsdeletevalue; Tasks: associatephone
Root: HKCR; Subkey: "CRMTogetherPhoneSage100"; ValueType: string; ValueName: ""; ValueData: "CRM Together Phone File - Sage100"; Flags: uninsdeletekey; Tasks: associatephone
Root: HKCR; Subkey: "CRMTogetherPhoneSage100\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: associatephone
Root: HKCR; Subkey: "CRMTogetherPhoneSage100\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associatephone

; Register crmtog-sage100:// protocol handler
Root: HKCR; Subkey: "crmtog-sage100"; ValueType: string; ValueName: ""; ValueData: "URL:CRM Together Protocol - Sage100"; Flags: uninsdeletekey; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog-sage100"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog-sage100\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog-sage100\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: registerprotocol

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"
Type: filesandordirs; Name: "{app}\temp"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Note: .NET Framework 4.8.1 check removed as IsDotNetDetected is not available
  // The application will show a clear error message if .NET Framework is missing
end;

function InitializeUninstall(): Boolean;
begin
  Result := True;
  
  // Check if application is running
  if CheckForMutexes('CRMTogether.PwaHost.Sage100') then
  begin
    if MsgBox('The application is currently running. Please close it before uninstalling.', 
              mbConfirmation, MB_OKCANCEL) = IDOK then
    begin
      Result := True;
    end
    else
    begin
      Result := False;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create logs directory
    ForceDirectories(ExpandConstant('{app}\logs'));
    
    // Create temp directory
    ForceDirectories(ExpandConstant('{app}\temp'));
  end;
end;
