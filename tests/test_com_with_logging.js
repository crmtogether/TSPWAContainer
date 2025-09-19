// Enhanced COM test script with detailed error reporting
// Run with: cscript //E:JScript //nologo "test_com_with_logging.js"

WScript.Echo("=== CRMTogether.PwaHost COM Test with Logging ===");
WScript.Echo("");

// Test 1: Check if COM object can be created
WScript.Echo("Test 1: Creating COM object...");
WScript.Echo("This should trigger the application to start with -Embedding parameter");
WScript.Echo("");

try {
    var ctrl = new ActiveXObject("CRMTogether.PwaHost");
    WScript.Echo("✓ COM object created successfully!");
    WScript.Echo("✓ The application should now be running in the background");
    WScript.Echo("");
    
    // Test 2: Test basic navigation
    WScript.Echo("Test 2: Testing navigation...");
    ctrl.Navigate("https://www.google.com");
    WScript.Echo("✓ Navigation command sent");
    WScript.Echo("");
    
    // Test 3: Test BringToFront
    WScript.Echo("Test 3: Testing BringToFront...");
    ctrl.BringToFront();
    WScript.Echo("✓ BringToFront command sent");
    WScript.Echo("");
    
    // Test 4: Test getting current URL
    WScript.Echo("Test 4: Testing GetCurrentUrl...");
    var url = ctrl.GetCurrentUrl();
    WScript.Echo("✓ Current URL: " + url);
    WScript.Echo("");
    
    // Test 5: Test setting size
    WScript.Echo("Test 5: Testing SetSize...");
    ctrl.SetSize(800, 600);
    WScript.Echo("✓ SetSize command sent");
    WScript.Echo("");
    
    WScript.Echo("=== All tests passed! ===");
    WScript.Echo("");
    WScript.Echo("Check the following for debugging:");
    WScript.Echo("1. Console output from the application");
    WScript.Echo("2. Log file: logs\\CRMTogether.PwaHost.log");
    WScript.Echo("3. Debug output in Visual Studio Output window");
    
} catch (e) {
    WScript.Echo("✗ COM object creation failed!");
    WScript.Echo("Error: " + e.message);
    WScript.Echo("Error Number: " + e.number);
    WScript.Echo("Error Description: " + e.description);
    WScript.Echo("");
    WScript.Echo("Troubleshooting steps:");
    WScript.Echo("1. Make sure the application is built");
    WScript.Echo("2. Run: CRMTogether.PwaHost.exe /RegServer");
    WScript.Echo("3. Check if the application is running");
    WScript.Echo("4. Verify COM registration in registry");
    WScript.Echo("5. Check the log file: logs\\CRMTogether.PwaHost.log");
    WScript.Echo("6. Look for console output from the application");
}

WScript.Echo("");
WScript.Echo("Press any key to continue...");
WScript.StdIn.ReadLine();
