
This is the code to sign the appbridge installer 

"C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign ^
  /n "Together Software Limited" ^
  /tr http://timestamp.comodoca.com ^
  /td sha256 /fd sha256 ^
  "C:\Marc\github\TSPWAContainer\src\CRMTogether.PwaHost\installer\SetupAppBridge_Sage100_Client.exe"

it must be run from the build machine 89.101.25.209

password/token is

iP3&uk6K^%dobcc


