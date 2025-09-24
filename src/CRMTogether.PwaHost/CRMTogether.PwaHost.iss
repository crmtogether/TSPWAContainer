; CRMTogether PWA Host Installer Script
; Generated for CRMTogether.PwaHost v2.0.0.0

#define MyAppName "CRM Together AppBridge"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "CRMTogether"
#define MyAppURL "https://crmtogether.com"
#define MyAppExeName "CRMTogether.PwaHost.exe"
#define MyAppDescription "WinForms WebView2 host with clipboard monitoring, custom URI handling, and multi-language support"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
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
OutputBaseFilename=SetupAppBridge_Client
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

Name: "registerprotocol"; Description: "Register crmtog:// protocol handler"; GroupDescription: "Protocol handlers:"; Flags: checkedonce

[Files]
; Main application executable
Source: "bin\x64\Release\net481\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\x64\Release\net481\{#MyAppExeName}.pdb"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; Application icon
Source: "bin\x64\Release\net481\images\crmtogethericon.ico"; DestDir: "{app}\images"; Flags: ignoreversion

; Translation files
Source: "bin\x64\Release\net481\translations\*.json"; DestDir: "{app}\translations"; Flags: ignoreversion

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
; Register crmtog:// protocol handler
Root: HKCR; Subkey: "crmtog"; ValueType: string; ValueName: ""; ValueData: "URL:CRM Together Protocol"; Flags: uninsdeletekey; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: registerprotocol

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
  if CheckForMutexes('CRMTogether.PwaHost') then
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

