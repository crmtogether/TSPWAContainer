@echo off
echo Building CRMTogether AppBridge for Sage100...
echo To execute from powershell ./build-sage100.bat 
echo.
echo NOTE: This script builds both app and installer in one step.
echo For manual signing workflow, use:
echo   1. build-sage100-app.bat
echo   2. (manually sign the executable)
echo   3. build-sage100-installer.bat
echo   4. (manually sign the installer)
echo.

REM Build Sage100 version
call build-installer.bat --environment Sage100 --config Release --platform x64

echo.
echo Sage100 build completed!
echo Target URL: https://appmxs100.crmtogether.com
echo.
