@echo off
REM Convenience script to build Sage100 installer only
REM This is equivalent to: build-installer-only.bat --environment Sage100
REM ./build-sage100-installer.bat

echo Building ContextAgent - Sage100 (Installer Only)
echo.

call build-installer-only.bat --environment Sage100

echo.
echo Now SIGN THE INSTALLER
echo.
cd "installer"
echo. "./installer/sign_s100_installer.bat"
