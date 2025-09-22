@echo off
echo Building CRMTogether PWA Host Installer...
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
dotnet build CRMTogether.PwaHost.csproj --configuration Release -p:Platform=x64
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
"%INNO_SETUP_PATH%" "CRMTogether.PwaHost.iss"
if %ERRORLEVEL% neq 0 (
    echo ERROR: Installer build failed!
    pause
    exit /b 1
)

echo.
echo Installer built successfully!
echo.
echo Installer location: installer\CRMTogether-PwaHost-Setup-v2.0.0.exe
echo.

REM Open the installer directory
explorer installer

pause
