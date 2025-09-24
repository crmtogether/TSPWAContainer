# Sage100 Build Configuration - Implementation Summary

## Overview
Successfully implemented a build configuration system that supports multiple environments, specifically including a Sage100 build that targets `https://appmxs100.crmtogether.com`.

## What Was Implemented

### 1. Build Configuration System
- **MSBuild Properties**: Added environment-specific build properties to `CRMTogether.PwaHost.csproj`
- **Build Constants**: Implemented `SAGE100_BUILD` preprocessor constant for Sage100 builds
- **Assembly Naming**: Different assembly names for different environments:
  - Default: `CRMTogether.PwaHost.exe`
  - Sage100: `CRMTogether.PwaHost.Sage100.exe`

### 2. Environment Configuration Files
Created `config/` directory with environment-specific JSON configurations:

#### `config/default.json`
```json
{
  "Environment": "Default",
  "StartupUrl": "https://crmtogether.com/univex-app-home/",
  "AppName": "CRM Together App Bridge",
  "ProtocolHandler": "crmtog"
}
```

#### `config/sage100.json`
```json
{
  "Environment": "Sage100",
  "StartupUrl": "https://appmxs100.crmtogether.com",
  "AppName": "CRM Together App Bridge - Sage100",
  "ProtocolHandler": "crmtog-sage100"
}
```

### 3. Enhanced AppConfig System
- **Environment Detection**: Automatically detects build environment using preprocessor constants
- **Configuration Loading**: Loads environment-specific configurations at runtime
- **URL Override**: Sage100 builds automatically use `https://appmxs100.crmtogether.com` as the startup URL

### 4. Build Scripts
#### Enhanced `build-installer.bat`
- Supports command-line parameters for environment, configuration, and platform
- Automatically selects appropriate installer script based on environment
- Usage examples:
  ```bash
  build-installer.bat --environment Sage100
  build-installer.bat --environment Default --config Debug
  ```

#### New `build-sage100.bat`
- Quick build script specifically for Sage100
- Simple one-command build for Sage100 environment

### 5. Sage100 Installer Configuration
Created `CRMTogether.PwaHost.Sage100.iss` with:
- **Unique App ID**: Different GUID to allow coexistence with default version
- **Sage100-specific branding**: "CRM Together App Bridge - Sage100"
- **Unique protocol handler**: `crmtog-sage100://`
- **Unique file associations**: `CRMTogetherEMLSage100`, `CRMTogetherPhoneSage100`
- **Target URL**: Points to `https://appmxs100.crmtogether.com`

### 6. Documentation
- **BUILD-README.md**: Comprehensive build system documentation
- **SAGE100-BUILD-SUMMARY.md**: This implementation summary

## How to Use

### Quick Sage100 Build
```bash
build-sage100.bat
```

### Advanced Build Options
```bash
# Sage100 build
build-installer.bat --environment Sage100 --config Release --platform x64

# Default build
build-installer.bat --environment Default --config Release --platform x64
```

### Manual Build Commands
```bash
# Sage100
dotnet build CRMTogether.PwaHost.csproj --configuration Release -p:Platform=x64 -p:Environment=Sage100

# Default
dotnet build CRMTogether.PwaHost.csproj --configuration Release -p:Platform=x64 -p:Environment=Default
```

## Output Files

### Sage100 Build
- **Executable**: `CRMTogether.PwaHost.Sage100.exe`
- **Installer**: `installer/CRMTogether-PwaHost-Sage100-Setup-v2.0.0.exe`
- **Target URL**: `https://appmxs100.crmtogether.com`

### Default Build
- **Executable**: `CRMTogether.PwaHost.exe`
- **Installer**: `installer/CRMTogether-PwaHost-Setup-v2.0.0.exe`
- **Target URL**: `https://crmtogether.com/univex-app-home/`

## Key Features

1. **Coexistence**: Both versions can be installed simultaneously
2. **Unique Identifiers**: Different App IDs, protocol handlers, and file associations
3. **Environment Detection**: Automatic configuration based on build constants
4. **Flexible Build System**: Easy to add new environments in the future
5. **Runtime Configuration**: Environment-specific settings loaded at application startup

## Testing Results
- ✅ Sage100 build compiles successfully
- ✅ Default build compiles successfully
- ✅ Configuration files are copied to output directory
- ✅ Environment-specific assembly names are generated
- ✅ Build scripts work with command-line parameters

The implementation is complete and ready for use. The Sage100 build will automatically target `https://appmxs100.crmtogether.com` and can coexist with the default version on the same system.
