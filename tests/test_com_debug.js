// Comprehensive COM test script for CRMTogether.PwaHost
// Run with: cscript //E:JScript //nologo "C:\Marc\github\TSPWAContainer\tests\test_com_debug.js"

WScript.Echo("=== CRMTogether.PwaHost COM Test ===");
WScript.Echo("");

// Test 1: Check if COM object can be created
WScript.Echo("Test 1: Creating COM object...");
try {
    var ctrl = new ActiveXObject("CRMTogether.PwaHost");
    WScript.Echo("✓ COM object created successfully!");
    
    // Test 2: Test basic navigation
    WScript.Echo("Test 2: Testing navigation...");
    ctrl.Navigate("https://www.google.com");
    WScript.Echo("✓ Navigation command sent");
    
    // Test 3: Test BringToFront
    WScript.Echo("Test 3: Testing BringToFront...");
    ctrl.BringToFront();
    WScript.Echo("✓ BringToFront command sent");
    
    // Test 4: Test getting current URL
    WScript.Echo("Test 4: Testing GetCurrentUrl...");
    var url = ctrl.GetCurrentUrl();
    WScript.Echo("✓ Current URL: " + url);
    
    // Test 5: Test setting size
    WScript.Echo("Test 5: Testing SetSize...");
    ctrl.SetSize(800, 600);
    WScript.Echo("✓ SetSize command sent");
    
    WScript.Echo("");
    WScript.Echo("=== All tests passed! ===");
    
} catch (e) {
    WScript.Echo("✗ COM object creation failed!");
    WScript.Echo("Error: " + e.message);
    WScript.Echo("Error Number: " + e.number);
    WScript.Echo("");
    WScript.Echo("Troubleshooting steps:");
    WScript.Echo("1. Make sure the application is built");
    WScript.Echo("2. Run: CRMTogether.PwaHost.exe /RegServer");
    WScript.Echo("3. Check if the application is running");
    WScript.Echo("4. Verify COM registration in registry");
}

WScript.Echo("");
WScript.Echo("Press any key to continue...");
WScript.StdIn.ReadLine();
