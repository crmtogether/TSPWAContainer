// to register the server
// ./CRMTogether.PwaHost.exe /RegServer

//cscript //E:JScript //nologo "C:\Marc\github\TSPWAContainer\tests\test nav.js"

try {
    var ctrl = new ActiveXObject("CRMTogether.PwaHost");
    ctrl.Navigate("https://example.com");
    ctrl.BringToFront();
    WScript.Echo("COM object created successfully!");
} catch (e) {
    WScript.Echo("COM object creation failed: " + e.message);
}