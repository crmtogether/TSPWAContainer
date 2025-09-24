# Installer Fixes - File Associations and Protocol Handlers

## Problem Identified
The Inno Setup installer scripts had several incorrect configurations:

1. **❌ Wrong File Associations**: The app was set to open `.eml` and `.phone` files directly
2. **❌ Wrong Protocol**: Sage100 installer used `crmtog-sage100://` instead of `crmtog://`
3. **❌ Incorrect App Behavior**: The app processes files but doesn't open them directly

## What the App Actually Does

### **File Processing (Not File Opening)**
- **.eml files**: The app **monitors** folders for .eml files and **processes** them when they appear
- **.phone files**: The app **monitors** folders for .phone files and **processes** them when they appear
- **NOT**: The app does **NOT** open these files directly when double-clicked

### **Protocol Handling**
- **URI**: Always `crmtog://` (not `crmtog-sage100://`)
- **Purpose**: Handle custom protocol calls from web applications
- **Examples**: `crmtog://context?value=email@example.com&source=clipboard`

## Fixes Applied

### **1. Removed Incorrect File Associations**

#### **Before (WRONG):**
```ini
[Tasks]
Name: "associateeml"; Description: "Associate .eml files with {#MyAppName}"; GroupDescription: "File associations:"; Flags: checkedonce
Name: "associatephone"; Description: "Associate .phone files with {#MyAppName}"; GroupDescription: "File associations:"; Flags: checkedonce

[Registry]
; Register .eml file association
Root: HKCR; Subkey: ".eml"; ValueType: string; ValueName: ""; ValueData: "CRMTogetherEML"; Flags: uninsdeletevalue; Tasks: associateeml
Root: HKCR; Subkey: "CRMTogetherEML"; ValueType: string; ValueName: ""; ValueData: "CRM Together EML File"; Flags: uninsdeletekey; Tasks: associateeml
Root: HKCR; Subkey: "CRMTogetherEML\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: associateeml
Root: HKCR; Subkey: "CRMTogetherEML\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associateeml

; Register .phone file association
Root: HKCR; Subkey: ".phone"; ValueType: string; ValueName: ""; ValueData: "CRMTogetherPhone"; Flags: uninsdeletevalue; Tasks: associatephone
Root: HKCR; Subkey: "CRMTogetherPhone"; ValueType: string; ValueName: ""; ValueData: "CRM Together Phone File"; Flags: uninsdeletekey; Tasks: associatephone
Root: HKCR; Subkey: "CRMTogetherPhone\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: associatephone
Root: HKCR; Subkey: "CRMTogetherPhone\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associatephone
```

#### **After (CORRECT):**
```ini
[Tasks]
; File association tasks removed - app doesn't open files directly

[Registry]
; File association registry entries removed
```

### **2. Fixed Protocol Handler**

#### **Sage100 Installer (BEFORE - WRONG):**
```ini
; Register crmtog-sage100:// protocol handler
Root: HKCR; Subkey: "crmtog-sage100"; ValueType: string; ValueName: ""; ValueData: "URL:CRM Together Protocol - Sage100"; Flags: uninsdeletekey; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog-sage100"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog-sage100\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog-sage100\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: registerprotocol
```

#### **Sage100 Installer (AFTER - CORRECT):**
```ini
; Register crmtog:// protocol handler
Root: HKCR; Subkey: "crmtog"; ValueType: string; ValueName: ""; ValueData: "URL:CRM Together Protocol"; Flags: uninsdeletekey; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\images\crmtogethericon.ico"; Tasks: registerprotocol
Root: HKCR; Subkey: "crmtog\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: registerprotocol
```

### **3. Updated Task Descriptions**

#### **Before:**
```ini
Name: "registerprotocol"; Description: "Register crmtog-sage100:// protocol handler"; GroupDescription: "Protocol handlers:"; Flags: checkedonce
```

#### **After:**
```ini
Name: "registerprotocol"; Description: "Register crmtog:// protocol handler"; GroupDescription: "Protocol handlers:"; Flags: checkedonce
```

## Files Modified

### **1. CRMTogether.PwaHost.Sage100.iss**
- ✅ **Removed**: `.eml` and `.phone` file association tasks and registry entries
- ✅ **Fixed**: Protocol handler from `crmtog-sage100://` to `crmtog://`
- ✅ **Updated**: Task description to reflect correct protocol

### **2. CRMTogether.PwaHost.iss**
- ✅ **Removed**: `.eml` and `.phone` file association tasks and registry entries
- ✅ **Kept**: `crmtog://` protocol handler (was already correct)

## How the App Actually Works

### **File Monitoring (Not File Opening)**
1. **App starts** and monitors configured folders
2. **Files appear** in monitored folders (e.g., Downloads)
3. **App detects** .eml or .phone files
4. **App processes** the files (extracts data, sends to web app)
5. **Files are moved** to processed folder

### **Protocol Handling**
1. **Web app** calls `crmtog://context?value=email@example.com&source=clipboard`
2. **Windows** launches the app with the URI as argument
3. **App processes** the URI and extracts parameters
4. **App sends** data to the web application

## Benefits of the Fix

### **✅ Correct Behavior**
- **No file associations** for .eml/.phone files (app doesn't open them)
- **Proper protocol handling** with `crmtog://` URI
- **Consistent** between Default and Sage100 builds

### **✅ User Experience**
- **No confusion** about what the app does
- **Proper protocol** registration for web integration
- **Clean installation** without unnecessary file associations

### **✅ Technical Accuracy**
- **Matches actual app behavior** (monitoring vs. opening)
- **Correct URI scheme** for all environments
- **Proper Windows integration** for protocol handling

## Testing the Fix

### **Installation Test**
1. **Run installer** with protocol handler option checked
2. **Verify** no .eml/.phone file associations are created
3. **Verify** `crmtog://` protocol is registered correctly

### **Functionality Test**
1. **Test protocol**: Try `crmtog://context?value=test&source=manual`
2. **Test file monitoring**: Drop .eml/.phone files in monitored folder
3. **Verify** files are processed (not opened directly)

The installer now correctly reflects what the application actually does: **protocol handling** and **file monitoring**, not **file opening**.
