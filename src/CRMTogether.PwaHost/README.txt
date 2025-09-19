CRMTogether.PwaHost — SINGLE INSTANCE + WM_COPYDATA forwarder + About box + menu layout fix

Fixes:
  - MenuStrip is now set as MainMenuStrip and docked before WebView2; layout re-applied so it no longer overlaps/cuts off the WebView.

Added:
  - About dialog under Settings → About... with app version and WebView2 runtime version.

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
