# Two-Stage Build Workflow for Manual Signing

## Overview
This workflow splits the build process into two stages to accommodate manual signing requirements:

1. **Stage 1**: Build the application executable
2. **Manual Step**: Sign the executable
3. **Stage 2**: Build the installer
4. **Manual Step**: Sign the installer

## Available Scripts

### **Stage 1: App Build Scripts**

#### **build-app-only.bat** (Generic)
```batch
# Build Default environment
build-app-only.bat

# Build Sage100 environment
build-app-only.bat --environment Sage100

# Build with custom options
build-app-only.bat --environment Sage100 --config Release --platform x64
```

#### **build-sage100-app.bat** (Convenience)
```batch
# Quick Sage100 app build
build-sage100-app.bat
```

### **Stage 2: Installer Build Scripts**

#### **build-installer-only.bat** (Generic)
```batch
# Build Default installer
build-installer-only.bat

# Build Sage100 installer
build-installer-only.bat --environment Sage100

# Build with custom options
build-installer-only.bat --environment Sage100 --config Release --platform x64
```

#### **build-sage100-installer.bat** (Convenience)
```batch
# Quick Sage100 installer build
build-sage100-installer.bat
```

## Complete Workflow Examples

### **Example 1: Sage100 Build with Manual Signing**

#### **Step 1: Build the Application**
```batch
build-sage100-app.bat
```
**Output**: `bin\x64\Release\net481\CRMTogether.PwaHost.Sage100.exe`

#### **Step 2: Manually Sign the Executable**
- Use your signing tool to sign: `bin\x64\Release\net481\CRMTogether.PwaHost.Sage100.exe`
- Verify the signature is applied correctly

#### **Step 3: Build the Installer**
```batch
build-sage100-installer.bat
```
**Output**: `installer\SetupAppBridge_Sage100_Client.exe`

#### **Step 4: Manually Sign the Installer**
- Use your signing tool to sign: `installer\SetupAppBridge_Sage100_Client.exe`
- Verify the signature is applied correctly

### **Example 2: Default Build with Manual Signing**

#### **Step 1: Build the Application**
```batch
build-app-only.bat
```
**Output**: `bin\x64\Release\net481\CRMTogether.PwaHost.exe`

#### **Step 2: Manually Sign the Executable**
- Use your signing tool to sign: `bin\x64\Release\net481\CRMTogether.PwaHost.exe`

#### **Step 3: Build the Installer**
```batch
build-installer-only.bat
```
**Output**: `installer\SetupAppBridge_Client.exe`

#### **Step 4: Manually Sign the Installer**
- Use your signing tool to sign: `installer\SetupAppBridge_Client.exe`

## File Locations

### **Built Executables**
| Environment | Executable Path |
|-------------|----------------|
| Default | `bin\x64\Release\net481\CRMTogether.PwaHost.exe` |
| Sage100 | `bin\x64\Release\net481\CRMTogether.PwaHost.Sage100.exe` |

### **Built Installers**
| Environment | Installer Path |
|-------------|----------------|
| Default | `installer\SetupAppBridge_Client.exe` |
| Sage100 | `installer\SetupAppBridge_Sage100_Client.exe` |

## Script Options

### **Common Options**
- `--environment [Default|Sage100]`: Build environment (default: Default)
- `--config [Debug|Release]`: Build configuration (default: Release)
- `--platform [x64|ARM64|AnyCPU]`: Build platform (default: x64)
- `--help`: Show help message

### **Examples**
```batch
# Debug build for Sage100
build-app-only.bat --environment Sage100 --config Debug

# ARM64 build for Default
build-app-only.bat --platform ARM64

# Custom combination
build-installer-only.bat --environment Sage100 --config Release --platform x64
```

## Error Handling

### **Stage 1 Errors**
- **Build failures**: Check .NET build output for compilation errors
- **Missing dependencies**: Ensure all NuGet packages are restored
- **Path issues**: Verify the project file is in the correct location

### **Stage 2 Errors**
- **Missing executable**: Run Stage 1 first
- **Inno Setup not found**: Install Inno Setup 6
- **Script not found**: Verify the .iss file exists for the environment

### **Signing Errors**
- **Tool not found**: Verify your signing tool is installed and in PATH
- **Certificate issues**: Check certificate validity and permissions
- **File locked**: Ensure the executable is not running

## Verification Steps

### **After Stage 1**
1. **File exists**: Check the executable was created
2. **File size**: Verify reasonable file size (not 0 bytes)
3. **Dependencies**: Ensure all DLLs are present in the output folder

### **After Signing Executable**
1. **Signature applied**: Use `signtool verify` or similar
2. **File integrity**: Ensure file wasn't corrupted during signing
3. **Permissions**: Verify file permissions are correct

### **After Stage 2**
1. **Installer exists**: Check the installer was created
2. **File size**: Verify reasonable installer size
3. **Dependencies**: Ensure installer contains all required files

### **After Signing Installer**
1. **Signature applied**: Use `signtool verify` or similar
2. **Test installation**: Install on a clean system
3. **Functionality**: Verify the installed app works correctly

## Troubleshooting

### **Common Issues**

#### **"Executable not found" in Stage 2**
- **Cause**: Stage 1 failed or wasn't run
- **Solution**: Run Stage 1 first and verify it completes successfully

#### **"Inno Setup not found"**
- **Cause**: Inno Setup 6 not installed
- **Solution**: Install Inno Setup 6 from https://jrsoftware.org/isinfo.php

#### **"Installer script not found"**
- **Cause**: .iss file missing for the environment
- **Solution**: Ensure `CRMTogether.PwaHost.iss` and `CRMTogether.PwaHost.Sage100.iss` exist

#### **Signing tool errors**
- **Cause**: Tool not found, certificate issues, or file permissions
- **Solution**: Check tool installation, certificate validity, and file permissions

### **Debug Mode**
Add `echo on` at the top of any script to see detailed command execution.

## Integration with CI/CD

### **Automated Stage 1**
```yaml
# Example GitHub Actions step
- name: Build Application
  run: build-app-only.bat --environment Sage100
```

### **Manual Signing Step**
- Use your organization's signing service
- Upload the executable for signing
- Download the signed executable

### **Automated Stage 2**
```yaml
# Example GitHub Actions step
- name: Build Installer
  run: build-installer-only.bat --environment Sage100
```

## Benefits of Two-Stage Build

### **✅ Manual Signing Support**
- **Flexible signing**: Use any signing tool or service
- **Certificate control**: Manage certificates separately from build
- **Audit trail**: Clear separation of build and signing steps

### **✅ Faster Iteration**
- **Skip signing**: Test unsigned builds quickly
- **Reuse builds**: Use same executable for multiple installer variants
- **Debug builds**: Build debug versions without signing

### **✅ Better Error Isolation**
- **Build errors**: Separate from signing issues
- **Signing errors**: Don't require rebuilding
- **Clear workflow**: Each step has specific purpose

### **✅ CI/CD Integration**
- **Automated builds**: Stage 1 can be automated
- **Manual approval**: Signing can require human approval
- **Flexible deployment**: Different signing for different environments
