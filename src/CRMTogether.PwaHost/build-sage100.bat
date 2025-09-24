@echo off
echo Building CRMTogether PWA Host for Sage100...
echo.

REM Build Sage100 version
call build-installer.bat --environment Sage100 --config Release --platform x64

echo.
echo Sage100 build completed!
echo Target URL: https://appmxs100.crmtogether.com
echo.
