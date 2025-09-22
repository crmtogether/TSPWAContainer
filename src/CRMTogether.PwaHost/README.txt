CRMTogether.PwaHost — SINGLE INSTANCE + WM_COPYDATA forwarder + About box + menu layout fix + File Processing System + Intelligent Clipboard Monitoring

Fixes:
  - MenuStrip is now set as MainMenuStrip and docked before WebView2; layout re-applied so it no longer overlaps/cuts off the WebView.

Added:
  - About dialog under Settings → About... with app version and WebView2 runtime version.
  - Automatic file processing system for .eml and .phone files
  - Last URL memory - app remembers and restores your last visited URL on startup
  - Safe file processing using processing/processed folder workflow
  - Intelligent clipboard monitoring with automatic content type detection (emails, phones, websites, addresses, text)
  - Real-time clipboard change notifications using Windows events (no polling)
  - Global keyboard hook for Ctrl+C detection across all applications
  - Debounced event processing to prevent excessive CPU usage

Compat tweaks:
  - Only uses Headers.SetHeader(...) (avoid AppendHeader for older WebView2).
  - Uses PrintSettings.Orientation instead of Landscape bool.

Build:
  - Open CRMTogether.PwaHost.csproj (net481). Build AnyCPU/x64/ARM64.

Self-registration:
  - App self-registers COM under HKCU and crmtog:// on first manual run if missing.
  - Optional commands: /RegServer, /UnregServer, /RegUri

Forwarding:
  - Second launches forward crmtog:// or --url=... to the running instance via WM_COPYDATA, then bring to front.

Sample crmtog:// URLs:

Basic Navigation:
  crmtog://navigate?url=https://www.google.com
  crmtog://?url=https://www.github.com

JavaScript Execution:
  crmtog://exec?js=alert('Hello from URI!')
  crmtog://exec?js=document.body.style.backgroundColor='lightblue'

JavaScript with Results:
  crmtog://execres?js=document.title
  crmtog://execres?js=window.location.href

Function Calls:
  crmtog://call?name=alert&args=["Function called from URI!"]
  crmtog://call?name=setTimeout&args=["() => alert('Delayed!')", 2000]

Script Execution:
  crmtog://script?name=myScript&args=arg1,arg2
  crmtog://?script=myScript

Home Page Management:
  crmtog://setHomePage?url=https://www.example.com

Note: JavaScript code in URIs must be URL-encoded. Special characters like quotes, spaces, and parentheses should be encoded as %27, %20, %28, %29, etc.

FILE PROCESSING SYSTEM:
======================

The app automatically monitors your Downloads folder (and other configured folders) for .eml and .phone files.
When files are detected, they are processed automatically using a safe copy-based workflow.

Supported File Types:
  - .eml files: Email files that trigger the changeSelectedEmail function in the web app
  - .phone files: Text files containing phone numbers that trigger the changeSelectedPhone function

File Processing Workflow:
  1. File Detection: App watches configured folders for new .eml/.phone files
  2. File Copying: Detected files are copied to a processing folder with unique timestamps
  3. File Processing: The copied file is processed (original file remains untouched)
  4. File Cleanup: Processed files are moved to a processed folder for reference

Folder Structure:
  %LocalAppData%\CRMTogether\PwaHost\
  ├── processing\          # Files currently being processed
  ├── processed\           # Successfully processed files (for reference)
  └── config.json         # App configuration

Configuration:
  - Go to Settings → Monitored Folders... to configure which folders to watch
  - Default: Your Downloads folder is automatically added
  - You can add additional folders to monitor

Benefits:
  - No file locking issues: Original files are never accessed during processing
  - Safe processing: Files are copied before processing, originals remain untouched
  - Crash prevention: Eliminates conflicts between file system watcher and processing
  - Automatic cleanup: Files are organized into processing/processed folders
  - Unique filenames: Timestamps prevent filename conflicts

LAST URL MEMORY:
===============

The app automatically remembers the last URL you visited and restores it when you restart the app.

URL Priority (in order):
  1. Command line URL (--url= parameter)
  2. Last visited URL (from previous session)
  3. Default startup URL (fallback)

This ensures you always return to where you left off, making the app more convenient to use.

PARAMETER MANAGEMENT SYSTEM:
============================

The app provides a flexible parameter management system that allows external integrations to build up context information before opening entities. This is particularly useful for CRM integrations where you need to pass contact information, addresses, phone numbers, etc.

Parameter Management Commands:
  crmtog://addParam?name=emailAddress&value=user@example.com
  crmtog://addParam?name=phoneNumber&value=+1234567890
  crmtog://addParam?name=address&value=3015 Lake Drive
  crmtog://addParam?name=name&value=John Doe
  crmtog://addParam?name=ContactName&value=Jane Smith

Parameter Management:
  crmtog://clearParams                    # Clear all stored parameters
  crmtog://getParams                      # Get current parameters (for debugging)

Entity Opening:
  crmtog://openEntity?entityType=contact&entityId=12345
  crmtog://openEntity?entityType=lead&entityId=67890
  crmtog://openEntity?entityType=opportunity&entityId=abc123

Usage Flow:
  1. Build Context: Call multiple addParam URIs to store context information
  2. Open Entity: Call openEntity with just the entity type and ID
  3. The app will use all stored parameters to populate the EML structure

Example Integration Flow:
  crmtog://addParam?name=emailAddress&value=john.doe@company.com
  crmtog://addParam?name=phoneNumber&value=+1-555-123-4567
  crmtog://addParam?name=address&value=123 Main Street, City, State 12345
  crmtog://addParam?name=name&value=John Doe
  crmtog://openEntity?entityType=contact&entityId=12345

The openEntity command will create an EML-like structure with all the stored parameters included in the customMessage object, making it available to the web application for processing.

CLIPBOARD MONITORING SYSTEM:
============================

The app includes intelligent clipboard monitoring that automatically detects and processes different types of content when copied or selected. This feature works across all applications and provides seamless integration with your CRM workflow.

Content Type Detection:
The system automatically detects and processes the following content types in order of priority:

1. Email Addresses:
   - Detects standard email formats (user@domain.com)
   - Triggers: crmtog://context?value=email@domain.com&source=clipboard&type=email
   - Stores as: emailAddress parameter

2. Phone Numbers:
   - Detects various phone formats: (555) 123-4567, 555-123-4567, +1 555 123 4567
   - Triggers: crmtog://phone?value=(555)123-4567&source=clipboard
   - Stores as: phoneNumber parameter

3. Websites/URLs:
   - Detects: https://example.com, www.example.com, example.com
   - Triggers: crmtog://website?value=https://example.com&source=clipboard
   - Stores as: website parameter

4. Postal Addresses:
   - Detects text containing newlines (multi-line addresses)
   - Triggers: crmtog://address?value=123 Main St\nApt 4B\nCity, State 12345&source=clipboard
   - Stores as: address parameter

5. Generic Text (Names, Companies):
   - Detects any other text that doesn't match the above patterns
   - Triggers: crmtog://text?value=John Smith&source=clipboard
   - Stores as: textValue parameter

How It Works:
- Real-time clipboard monitoring using Windows clipboard change notifications
- Global keyboard hook detects Ctrl+C operations from any application
- Event-based processing (no constant polling for better performance)
- Debouncing prevents rapid-fire events (500ms cooldown)
- Automatic content type detection and routing

Menu Controls:
- Clipboard → Toggle Clipboard Monitoring: Enable/disable monitoring
- Clipboard → Test Clipboard Check: Manually test current clipboard content
- Menu text shows current state: "Enable/Disable Clipboard Monitoring"

Configuration:
- Monitoring is enabled by default
- Can be toggled on/off via the Clipboard menu
- Status messages appear in the application status bar
- Debug logging available at: %LOCALAPPDATA%\CRMTogether\PwaHost\debug.log

Use Cases:
- Copy a contact's email → Automatically creates email context
- Copy a phone number → Triggers phone lookup/search
- Copy a company website → Opens website context
- Copy a person's name → Triggers name/contact search
- Copy a company name → Triggers company lookup
- Copy a postal address → Triggers address processing

Integration with Existing Systems:
- All detected content is processed through the existing URI command system
- Uses the same OpenEntity method for EML building
- Integrates with the parameter management system
- Works seamlessly with the existing changeSelectedEmail browser function

Performance:
- Event-driven (no constant polling)
- Minimal CPU usage
- Debounced to prevent excessive processing
- Only processes when clipboard actually changes
