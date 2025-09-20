CRMTogether.PwaHost — SINGLE INSTANCE + WM_COPYDATA forwarder + About box + menu layout fix + File Processing System

Fixes:
  - MenuStrip is now set as MainMenuStrip and docked before WebView2; layout re-applied so it no longer overlaps/cuts off the WebView.

Added:
  - About dialog under Settings → About... with app version and WebView2 runtime version.
  - Automatic file processing system for .eml and .phone files
  - Last URL memory - app remembers and restores your last visited URL on startup
  - Safe file processing using processing/processed folder workflow

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
