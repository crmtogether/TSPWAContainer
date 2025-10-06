"C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign ^
  /n "Together Software Limited" ^
  /tr http://timestamp.comodoca.com ^
  /td sha256 /fd sha256 ^
  "C:\Marc\github\TSPWAContainer\src\CRMTogether.PwaHost\installer\SetupContextAgent_Sage100_Client.exe"	