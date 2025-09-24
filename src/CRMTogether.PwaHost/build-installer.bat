@echo off
echo Building CRMTogether PWA Host Installer...
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

REM Check if Inno Setup is installed
set "INNO_SETUP_PATH="
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files (x86)\Inno Setup 5\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 5\ISCC.exe" (
    set "INNO_SETUP_PATH=C:\Program Files\Inno Setup 5\ISCC.exe"
) else (
    echo ERROR: Inno Setup not found!
    echo Please install Inno Setup from: https://jrsoftware.org/isinfo.php
    echo.
    pause
    exit /b 1
)

echo Found Inno Setup at: %INNO_SETUP_PATH%
echo.

REM Build the project first
echo Building the project...
dotnet build CRMTogether.PwaHost.csproj --configuration %BUILD_CONFIGURATION% -p:Platform=%BUILD_PLATFORM% -p:Environment=%BUILD_ENVIRONMENT%
if %ERRORLEVEL% neq 0 (
    echo ERROR: Project build failed!
    pause
    exit /b 1
)

echo.
echo Project built successfully!
echo.

REM Create installer directory
if not exist "installer" mkdir installer

REM Build the installer
echo Building installer...
if /i "%BUILD_ENVIRONMENT%"=="Sage100" (
    echo Using Sage100 installer script...
    "%INNO_SETUP_PATH%" "CRMTogether.PwaHost.Sage100.iss"
    set "INSTALLER_NAME=CRMTogether-PwaHost-Sage100-Setup-v2.0.0.exe"
) else (
    echo Using default installer script...
    "%INNO_SETUP_PATH%" "CRMTogether.PwaHost.iss"
    set "INSTALLER_NAME=CRMTogether-PwaHost-Setup-v2.0.0.exe"
)

if %ERRORLEVEL% neq 0 (
    echo ERROR: Installer build failed!
    pause
    exit /b 1
)

echo.
echo Installer built successfully!
echo.
echo Installer location: installer\%INSTALLER_NAME%
echo.

REM Open the installer directory
explorer installer

pause
