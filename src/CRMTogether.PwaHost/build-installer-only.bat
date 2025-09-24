@echo off
REM Build Installer Only - Second Part of Two-Stage Build Process
REM This script creates the installer from the already-built and signed application
REM Run this AFTER manually signing the executable from build-app-only.bat

echo ========================================
echo CRM Together AppBridge - Installer Build Only
echo ========================================
echo.

REM Parse command line arguments
set "BUILD_ENVIRONMENT=Default"
set "BUILD_CONFIGURATION=Release"
set "BUILD_PLATFORM=x64"

:parse_args
if "%~1"=="" goto :args_done
if /i "%~1"=="--environment" (
    set "BUILD_ENVIRONMENT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--config" (
    set "BUILD_CONFIGURATION=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--platform" (
    set "BUILD_PLATFORM=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--help" (
    echo Usage: %0 [options]
    echo Options:
    echo   --environment [Default^|Sage100]  Build environment (default: Default)
    echo   --config [Debug^|Release]         Build configuration (default: Release)
    echo   --platform [x64^|ARM64^|AnyCPU]   Build platform (default: x64)
    echo   --help                           Show this help message
    echo.
    echo Examples:
    echo   %0 --environment Sage100
    echo   %0 --environment Default --config Debug
    echo.
    echo This script builds the installer only.
    echo Run this AFTER manually signing the executable from build-app-only.bat
    exit /b 0
)
shift
goto :parse_args

:args_done

echo Build Configuration:
echo   Environment: %BUILD_ENVIRONMENT%
echo   Configuration: %BUILD_CONFIGURATION%
echo   Platform: %BUILD_PLATFORM%
echo.

REM Check if Inno Setup is available
set "INNO_SETUP_PATH="
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files\Inno Setup 6\ISCC.exe"
) else (
    echo ERROR: Inno Setup not found!
    echo Please install Inno Setup 6 from: https://jrsoftware.org/isinfo.php
    echo.
    pause
    exit /b 1
)

echo Found Inno Setup at: %INNO_SETUP_PATH%
echo.

REM Determine the executable name and path based on environment
if /i "%BUILD_ENVIRONMENT%"=="Sage100" (
    set "EXE_NAME=CRMTogether.PwaHost.Sage100.exe"
    set "EXE_PATH=bin\%BUILD_PLATFORM%\%BUILD_CONFIGURATION%\net481\%EXE_NAME%"
    set "INSTALLER_SCRIPT=CRMTogether.PwaHost.Sage100.iss"
    set "INSTALLER_NAME=SetupAppBridge_Sage100_Client.exe"
) else (
    set "EXE_NAME=CRMTogether.PwaHost.exe"
    set "EXE_PATH=bin\%BUILD_PLATFORM%\%BUILD_CONFIGURATION%\net481\%EXE_NAME%"
    set "INSTALLER_SCRIPT=CRMTogether.PwaHost.iss"
    set "INSTALLER_NAME=SetupAppBridge_Client.exe"
)

echo Expected executable: %EXE_PATH%
echo Installer script: %INSTALLER_SCRIPT%
echo Output installer: installer\%INSTALLER_NAME%
echo.

REM Verify the executable exists
if not exist "%EXE_PATH%" (
    echo ERROR: Executable not found at: %EXE_PATH%
    echo.
    echo Please run build-app-only.bat first to build the application.
    echo Then manually sign the executable before running this script.
    echo.
    pause
    exit /b 1
)

REM Verify the installer script exists
if not exist "%INSTALLER_SCRIPT%" (
    echo ERROR: Installer script not found: %INSTALLER_SCRIPT%
    echo.
    pause
    exit /b 1
)

REM Create installer directory if it doesn't exist
if not exist "installer" (
    echo Creating installer directory...
    mkdir installer
)

echo ========================================
echo BUILDING INSTALLER
echo ========================================
echo.

echo Building installer using Inno Setup...
echo Command: "%INNO_SETUP_PATH%" "%INSTALLER_SCRIPT%"
echo.

"%INNO_SETUP_PATH%" "%INSTALLER_SCRIPT%"

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Installer build failed with exit code %ERRORLEVEL%
    echo Please check the Inno Setup output above for errors.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================
echo INSTALLER BUILD COMPLETED SUCCESSFULLY
echo ========================================
echo.

REM Verify the installer was created
set "INSTALLER_PATH=installer\%INSTALLER_NAME%"
if not exist "%INSTALLER_PATH%" (
    echo ERROR: Expected installer not found at: %INSTALLER_PATH%
    echo Please check the Inno Setup output above for errors.
    pause
    exit /b 1
)

echo Built installer: %INSTALLER_PATH%
echo.

REM Get file sizes for verification
for %%F in ("%EXE_PATH%") do set "EXE_SIZE=%%~zF"
for %%F in ("%INSTALLER_PATH%") do set "INSTALLER_SIZE=%%~zF"

echo File sizes:
echo   Executable: %EXE_SIZE% bytes
echo   Installer:  %INSTALLER_SIZE% bytes
echo.

echo ========================================
echo NEXT STEPS
echo ========================================
echo.
echo 1. MANUALLY SIGN the installer:
echo    File: %INSTALLER_PATH%
echo.
echo 2. Test the installer on a clean system
echo.
echo 3. Distribute the signed installer
echo.

echo Press any key to exit...
pause >nul
