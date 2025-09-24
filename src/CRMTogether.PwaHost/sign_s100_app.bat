"C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign ^
  /n "Together Software Limited" ^
  /tr http://timestamp.comodoca.com ^
  /td sha256 /fd sha256 ^
  "C:\Marc\github\TSPWAContainer\src\CRMTogether.PwaHost\bin\x64\Release\net481\CRMTogether.PwaHost.Sage100.exe"