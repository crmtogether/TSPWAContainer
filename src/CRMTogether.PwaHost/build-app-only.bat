@echo off
REM Build App Only - First Part of Two-Stage Build Process
REM This script builds the application but does NOT create the installer
REM After this completes, manually sign the executable, then run build-installer-only.bat

echo ========================================
echo CRM Together AppBridge - App Build Only
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
    echo This script builds the application only.
    echo After building, manually sign the executable, then run build-installer-only.bat
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

REM Check if Inno Setup is available (not needed for app build, but good to check)
set "INNO_SETUP_PATH="
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files\Inno Setup 6\ISCC.exe"
) else (
    echo Warning: Inno Setup not found. Installer build will fail.
    echo Please install Inno Setup 6 from: https://jrsoftware.org/isinfo.php
    echo.
)

REM Build the application
echo Building application...
echo Command: dotnet build CRMTogether.PwaHost.csproj --configuration %BUILD_CONFIGURATION% -p:Platform=%BUILD_PLATFORM% -p:Environment=%BUILD_ENVIRONMENT%
echo.

dotnet build CRMTogether.PwaHost.csproj --configuration %BUILD_CONFIGURATION% -p:Platform=%BUILD_PLATFORM% -p:Environment=%BUILD_ENVIRONMENT%

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed with exit code %ERRORLEVEL%
    echo Please fix the build errors and try again.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================
echo BUILD COMPLETED SUCCESSFULLY
echo ========================================
echo.

REM Determine the executable name based on environment
if /i "%BUILD_ENVIRONMENT%"=="Sage100" (
    set "EXE_NAME=CRMTogether.PwaHost.Sage100.exe"
    set "EXE_PATH=bin\%BUILD_PLATFORM%\%BUILD_CONFIGURATION%\net481\%EXE_NAME%"
) else (
    set "EXE_NAME=CRMTogether.PwaHost.exe"
    set "EXE_PATH=bin\%BUILD_PLATFORM%\%BUILD_CONFIGURATION%\net481\%EXE_NAME%"
)

echo Built executable: %EXE_PATH%
echo.

if not exist "%EXE_PATH%" (
    echo ERROR: Expected executable not found at: %EXE_PATH%
    echo Please check the build output above for errors.
    pause
    exit /b 1
)

echo ========================================
echo NEXT STEPS
echo ========================================
echo.
echo 1. MANUALLY SIGN the executable:
echo    File: %EXE_PATH%
echo.
echo 2. After signing, run the installer build:
if /i "%BUILD_ENVIRONMENT%"=="Sage100" (
    echo    build-installer-only.bat --environment Sage100
) else (
    echo    build-installer-only.bat --environment Default
)
echo.
echo 3. After installer is built, MANUALLY SIGN the installer:
if /i "%BUILD_ENVIRONMENT%"=="Sage100" (
    echo    File: installer\SetupAppBridge_Sage100_Client.exe
) else (
    echo    File: installer\SetupAppBridge_Client.exe
)
echo.

echo Press any key to exit...
pause >nul
