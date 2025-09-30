# CRMTogether PWA Host Installer

This directory contains the Inno Setup script and build tools for creating a Windows installer for the CRMTogether PWA Host application.

## Files

- **`CRMTogether.PwaHost.iss`** - Inno Setup script file
- **`build-installer.bat`** - Batch file to build the installer
- **`INSTALLER-README.md`** - This documentation file

## Prerequisites

1. **Inno Setup** - Download and install from: https://jrsoftware.org/isinfo.php
   - Supports Inno Setup 5.x and 6.x
   - The batch file will automatically detect the installation path

2. **.NET Framework 4.8.1** - Required for the application to run
   - The installer will check for this dependency

3. **Built Application** - The project must be built in Release mode
   - The batch file will build it automatically

## Building the Installer

### Option 1: Using the Batch File (Recommended)

1. Run `build-installer.bat` from the project root directory
2. The script will:
   - Check for Inno Setup installation
   - Build the project in Release mode
   - Create the installer
   - Open the installer directory

### Option 2: Manual Build

1. Build the project:
   ```cmd
   dotnet build CRMTogether.PwaHost.csproj --configuration Release --platform x64
   ```

2. Run Inno Setup Compiler:
   ```cmd
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "CRMTogether.PwaHost.iss"
   ```

## Installer Features

### Application Information
- **Name**: ContextAgent
- **Version**: 2.0.0
- **Publisher**: CRMTogether
- **Description**: WinForms WebView2 host with clipboard monitoring, custom URI handling, and multi-language support

### Installation Options
- **Desktop Icon**: Optional desktop shortcut
- **Quick Launch Icon**: Optional quick launch shortcut (Windows 7 and earlier)
- **File Associations**: 
  - `.eml` files → ContextAgent
  - `.phone` files → ContextAgent
- **Protocol Handler**: `crmtog://` protocol registration

### Multi-Language Support
- English (default)
- German
- French

### System Requirements
- Windows 7 SP1 or later
- .NET Framework 4.8.1 or later
- x64 or ARM64 architecture

### Installation Directory
- Default: `C:\Program Files\ContextAgent\`
- User can change during installation

## Installer Contents

The installer includes:
- Main application executable and configuration
- All required .NET dependencies
- WebView2 runtime files for all architectures
- Translation files (English, German, French)
- Application icon
- Documentation (README.txt)

## Registry Entries

The installer creates the following registry entries (optional, user-selectable):

### File Associations
- `.eml` → `CRMTogetherEML` file type
- `.phone` → `CRMTogetherPhone` file type

### Protocol Handler
- `crmtog://` → CRM Together Protocol

## Uninstallation

The installer creates a complete uninstaller that:
- Removes all application files
- Removes registry entries (if created during installation)
- Removes shortcuts
- Cleans up logs and temp directories

## Customization

To customize the installer:

1. **Change Application Details**: Edit the `#define` section at the top of `CRMTogether.PwaHost.iss`
2. **Add/Remove Files**: Modify the `[Files]` section
3. **Change Registry Entries**: Modify the `[Registry]` section
4. **Add Custom Code**: Modify the `[Code]` section

## Troubleshooting

### "Inno Setup not found"
- Install Inno Setup from the official website
- Ensure it's installed in the standard location

### ".NET Framework 4.8.1 not found"
- Install .NET Framework 4.8.1 from Microsoft
- The installer will show this error if the framework is missing

### "Application is running"
- Close the application before uninstalling
- The uninstaller will prompt you to close it

### Build Errors
- Ensure the project builds successfully in Release mode
- Check that all required files exist in the output directory
- Verify file paths in the Inno Setup script

## Output

The installer will be created as:
`installer\CRMTogether-PwaHost-Setup-v2.0.0.exe`

The installer is digitally signed (if you have a code signing certificate) and ready for distribution.
