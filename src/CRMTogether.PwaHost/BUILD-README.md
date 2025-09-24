# CRMTogether PWA Host - Build System

This project supports multiple build configurations for different environments and target URLs.

## Build Configurations

### Default Build
- **Target URL**: https://crmtogether.com/univex-app-home/
- **Assembly Name**: CRMTogether.PwaHost
- **Protocol Handler**: crmtog://

### Sage100 Build
- **Target URL**: https://appmxs100.crmtogether.com
- **Assembly Name**: CRMTogether.PwaHost.Sage100
- **Protocol Handler**: crmtog-sage100://

## Building

### Quick Build Commands

#### Default Build
```bash
build-installer.bat
```

#### Sage100 Build
```bash
build-sage100.bat
```

### Advanced Build Options

The `build-installer.bat` script supports the following parameters:

```bash
build-installer.bat [options]

Options:
  --environment [Default|Sage100]  Build environment (default: Default)
  --config [Debug|Release]         Build configuration (default: Release)
  --platform [x64|ARM64|AnyCPU]   Build platform (default: x64)
  --help                           Show help message

Examples:
  build-installer.bat --environment Sage100
  build-installer.bat --environment Default --config Debug
  build-installer.bat --environment Sage100 --config Release --platform x64
```

### Manual Build Commands

#### Default Environment
```bash
dotnet build CRMTogether.PwaHost.csproj --configuration Release -p:Platform=x64 -p:Environment=Default
```

#### Sage100 Environment
```bash
dotnet build CRMTogether.PwaHost.csproj --configuration Release -p:Platform=x64 -p:Environment=Sage100
```

## Configuration Files

Environment-specific configurations are stored in the `config/` directory:

- `config/default.json` - Default environment configuration
- `config/sage100.json` - Sage100 environment configuration

These files contain:
- Target URLs
- Application names and descriptions
- Protocol handlers
- File associations
- Build information

## Output Files

### Default Build
- **Executable**: `CRMTogether.PwaHost.exe`
- **Installer**: `installer/CRMTogether-PwaHost-Setup-v2.0.0.exe`

### Sage100 Build
- **Executable**: `CRMTogether.PwaHost.Sage100.exe`
- **Installer**: `installer/CRMTogether-PwaHost-Sage100-Setup-v2.0.0.exe`

## Adding New Environments

To add a new environment:

1. Create a new configuration file in `config/` (e.g., `config/myenv.json`)
2. Add environment-specific properties to `CRMTogether.PwaHost.csproj`
3. Create a new installer script (e.g., `CRMTogether.PwaHost.MyEnv.iss`)
4. Update the build script to handle the new environment

## Build Constants

The build system uses preprocessor constants to determine the environment:

- `SAGE100_BUILD` - Defined when building for Sage100 environment
- Additional constants can be added for other environments

## File Associations

Each environment can have its own file associations:

- **Default**: `.eml` → `CRMTogetherEML`, `.phone` → `CRMTogetherPhone`
- **Sage100**: `.eml` → `CRMTogetherEMLSage100`, `.phone` → `CRMTogetherPhoneSage100`

## Protocol Handlers

Each environment registers its own protocol handler:

- **Default**: `crmtog://`
- **Sage100**: `crmtog-sage100://`

This allows multiple versions of the application to coexist on the same system.
