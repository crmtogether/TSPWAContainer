# Visual Studio Debug Guide - Environment Configuration

## Overview
The application now supports multiple ways to configure the environment for debugging in Visual Studio. The environment detection follows this priority order:

1. **Build Constants** (MSBuild) - Highest priority
2. **Environment Variable** - Medium priority  
3. **Command Line Arguments** - Low priority
4. **Default** - Fallback

## Method 1: Environment Variable (Recommended for Debug)

### Step 1: Set Environment Variable
1. **Right-click project** → **Properties**
2. Go to **Debug** → **General**
3. Click **Open debug launch profiles UI**
4. Add environment variable:
   - **Name**: `Environment`
   - **Value**: `Sage100` (or `Default`)

### Step 2: Start Debugging
- Press **F5** or click **Debug** → **Start Debugging**

## Method 2: Command Line Arguments

### Step 1: Set Command Line Arguments
1. **Right-click project** → **Properties**
2. Go to **Debug** → **General**
3. In **Command line arguments**, add:
   ```
   --environment Sage100
   ```
   or
   ```
   -e Sage100
   ```

### Step 2: Start Debugging
- Press **F5** or click **Debug** → **Start Debugging**

## Method 3: MSBuild Properties (Advanced)

### Step 1: Set MSBuild Properties
1. **Right-click project** → **Properties**
2. Go to **Build** → **General**
3. In **Conditional compilation symbols**, add: `SAGE100_BUILD`

### Step 2: Start Debugging
- Press **F5** or click **Debug** → **Start Debugging**

## Method 4: launchSettings.json (Professional)

### Step 1: Create launchSettings.json
Create file: `Properties/launchSettings.json`
```json
{
  "profiles": {
    "Default": {
      "commandName": "Project",
      "environmentVariables": {
        "Environment": "Default"
      }
    },
    "Sage100": {
      "commandName": "Project",
      "environmentVariables": {
        "Environment": "Sage100"
      }
    }
  }
}
```

### Step 2: Select Profile
1. In Visual Studio, click the **Debug** dropdown next to the play button
2. Select **Sage100** profile
3. Press **F5**

## Verification Methods

### Debug Output Verification
When the application starts, check the **Output** window for these messages:

**For Sage100:**
```
Environment detected from environment variable: Sage100
Loading environment config from: C:\...\config\sage100.json
Loaded environment config for: Sage100
Applied environment config: Sage100
Folder monitoring is disabled for this environment, skipping watcher creation
Clipboard monitoring disabled for this environment
```

**For Default:**
```
Environment detected from environment variable: Default
Loading environment config from: C:\...\config\default.json
Loaded environment config for: Default
Applied environment config: Default
```

### Visual Verification
**Sage100 Build Should Show:**
- ❌ **No "Test" menu**
- ❌ **No "Clipboard" menu**
- ❌ **No "Monitored folders..." menu** (in Settings)
- ✅ **Startup URL**: `https://appmxs100.crmtogether.com`
- ✅ **Window Title**: "CRM Together App Bridge - Sage100"

**Default Build Should Show:**
- ✅ **"Test" menu** visible
- ✅ **"Clipboard" menu** visible
- ✅ **"Monitored folders..." menu** visible (in Settings)
- ✅ **Startup URL**: `https://crmtogether.com/univex-app-home/`
- ✅ **Window Title**: "CRM Together App Bridge"

## Troubleshooting

### Issue: Environment Not Detected
**Symptoms:**
- Application shows Default behavior even when Sage100 is configured
- Debug output shows "Environment detected from environment variable: Default"

**Solutions:**
1. **Check Environment Variable**: Ensure `Environment=Sage100` is set correctly
2. **Check Command Line Args**: Ensure `--environment Sage100` is set correctly
3. **Check Config Files**: Ensure `config/sage100.json` exists in output directory
4. **Check Debug Output**: Look for error messages in the Output window

### Issue: Config File Not Found
**Symptoms:**
- Debug output shows "Environment config file not found"
- Application falls back to Default behavior

**Solutions:**
1. **Check File Location**: Ensure `config/sage100.json` is in the output directory
2. **Check File Permissions**: Ensure the application can read the config file
3. **Check File Content**: Ensure the JSON file is valid

### Issue: Feature Toggles Not Working
**Symptoms:**
- Menus are still visible when they should be hidden
- Folder monitoring is still active

**Solutions:**
1. **Check Config Loading**: Look for "Applied environment config: Sage100" in debug output
2. **Check Feature Toggles**: Verify the config file has the correct boolean values
3. **Check Code**: Ensure the conditional logic is working correctly

## Quick Setup for Sage100 Debug

### Fastest Method (Environment Variable):
1. **Right-click project** → **Properties**
2. **Debug** → **Environment variables**
3. Add: `Environment` = `Sage100`
4. **Press F5**

### Alternative Method (Command Line):
1. **Right-click project** → **Properties**
2. **Debug** → **Command line arguments**
3. Add: `--environment Sage100`
4. **Press F5**

## Debug Output Locations

### Visual Studio Output Window
- **View** → **Output**
- Select **Debug** from the dropdown
- Look for environment detection messages

### Debug Log File
- **Location**: `%LOCALAPPDATA%\CRMTogether\PwaHost\debug.log`
- **Content**: Detailed logging of environment detection and configuration loading

## Environment Detection Priority

The application checks for environment configuration in this order:

1. **Build Constants** (`#if SAGE100_BUILD`)
   - Set during MSBuild compilation
   - Highest priority
   - Used for production builds

2. **Environment Variable** (`Environment=Sage100`)
   - Set in Visual Studio debug settings
   - Medium priority
   - Used for development debugging

3. **Command Line Arguments** (`--environment Sage100`)
   - Set in Visual Studio debug settings
   - Low priority
   - Used for development debugging

4. **Default** (`Default`)
   - Fallback when no other method is detected
   - Lowest priority
   - Used when no configuration is found

## Best Practices

### For Development
- Use **Environment Variable** method for consistent debugging
- Use **launchSettings.json** for multiple environment profiles
- Always verify with debug output

### For Production
- Use **MSBuild Properties** for build-time configuration
- Use **Command Line Arguments** for runtime configuration
- Test both Default and Sage100 builds

### For Testing
- Test both environment configurations
- Verify feature toggles work correctly
- Check debug output for proper environment detection
- Verify startup URLs are correct for each environment
