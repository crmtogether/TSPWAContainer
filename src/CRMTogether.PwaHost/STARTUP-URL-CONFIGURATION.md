# Startup URL Configuration - Always Use Environment Settings

## Overview
Successfully modified the application to always use the startup URL from the configured environment settings, ensuring consistent behavior across different builds and preventing URL overrides.

## Changes Made

### 1. Modified Program.cs Startup Logic
**File**: `Program.cs`

**Before**: The application used a priority system for determining the startup URL:
1. Command line URL (`--url=` parameter)
2. Last URL (from saved user config)
3. Startup URL (from environment config)

**After**: The application now always uses the configured startup URL from environment settings:
```csharp
// Always use the configured startup URL from environment settings
initialUrl = Config.StartupUrl;
```

### 2. Enhanced AppConfig Environment Override
**File**: `AppConfig.cs`

**Before**: Environment startup URL was only applied if no user startup URL was set:
```csharp
// Override startup URL if not already set
if (string.IsNullOrWhiteSpace(appConfig.StartupUrl))
{
    appConfig.StartupUrl = environmentConfig.StartupUrl;
}
```

**After**: Environment startup URL always takes precedence:
```csharp
// Always use the environment-specific startup URL
appConfig.StartupUrl = environmentConfig.StartupUrl;
```

### 3. Disabled Web Application URL Changes
**File**: `PwaHostObject.cs`

**Before**: The web application could change the startup URL via `SetHomePage()`:
```csharp
public string SetHomePage(string url)
{
    Program.Config.StartupUrl = url;
    Program.Config.Save();
    return $"Home page set to: {url}";
}
```

**After**: The web application can no longer change the startup URL:
```csharp
public string SetHomePage(string url)
{
    // Startup URL is now controlled by environment configuration and cannot be changed
    return "Startup URL is controlled by environment configuration and cannot be changed";
}
```

## Behavior by Environment

### Default Build
- **Startup URL**: Always `https://crmtogether.com/univex-app-home/`
- **Source**: `config/default.json`
- **Override**: Not possible via command line, user settings, or web application

### Sage100 Build
- **Startup URL**: Always `https://appmxs100.crmtogether.com`
- **Source**: `config/sage100.json`
- **Override**: Not possible via command line, user settings, or web application

## Benefits

### 1. **Consistent Behavior**
- Each build environment always starts with its intended URL
- No confusion from command line parameters or user settings
- Predictable startup behavior for different deployments

### 2. **Security & Control**
- Prevents unauthorized URL changes via web application
- Ensures users always land on the correct environment-specific URL
- Maintains separation between different build configurations

### 3. **Deployment Reliability**
- Build-specific URLs are guaranteed to be used
- No dependency on user configuration files
- Environment-specific behavior is enforced at the application level

### 4. **Simplified Configuration**
- Single source of truth for startup URLs (environment config files)
- No complex priority logic to maintain
- Clear separation between build-time and runtime configuration

## Configuration Files

### Default Environment (`config/default.json`)
```json
{
  "StartupUrl": "https://crmtogether.com/univex-app-home/"
}
```

### Sage100 Environment (`config/sage100.json`)
```json
{
  "StartupUrl": "https://appmxs100.crmtogether.com"
}
```

## Testing Results

### Build Tests
- ✅ Sage100 build compiles successfully
- ✅ Default build compiles successfully
- ✅ Configuration files are copied to output directory
- ✅ Environment-specific startup URLs are enforced

### Behavior Tests
- ✅ Default build always starts with `https://crmtogether.com/univex-app-home/`
- ✅ Sage100 build always starts with `https://appmxs100.crmtogether.com`
- ✅ Command line `--url=` parameter is ignored
- ✅ User configuration startup URL is ignored
- ✅ Web application `SetHomePage()` cannot change startup URL

## Usage

### Building with Environment-Specific URLs
```bash
# Default build - always starts with https://crmtogether.com/univex-app-home/
build-installer.bat --environment Default

# Sage100 build - always starts with https://appmxs100.crmtogether.com
build-installer.bat --environment Sage100
```

### Changing Startup URLs
To change the startup URL for a specific environment, modify the corresponding configuration file:

1. **For Default builds**: Edit `config/default.json`
2. **For Sage100 builds**: Edit `config/sage100.json`
3. **Rebuild** the application with the appropriate environment

## Migration Notes

### Breaking Changes
- Command line `--url=` parameter no longer affects startup URL
- User configuration startup URL is no longer used
- Web application `SetHomePage()` function no longer changes startup URL

### Backward Compatibility
- Existing configuration files will continue to work
- Environment-specific URLs will override any existing user settings
- No data loss - only behavior change in URL determination

## Future Considerations

### Adding New Environments
To add a new environment with its own startup URL:

1. Create new configuration file in `config/` directory
2. Add environment-specific build properties to `.csproj`
3. Update build scripts to handle new environment
4. The startup URL will automatically be enforced for the new environment

### URL Validation
Consider adding URL validation in the configuration loading to ensure valid URLs are provided in environment configuration files.

The implementation is complete and provides reliable, environment-specific startup URL behavior.
