@echo off
REM Convenience script to build Sage100 app only
REM This is equivalent to: build-app-only.bat --environment Sage100
REM ./build-sage100-app.bat

echo Building ContextAgent - Sage100 (App Only)
echo.

call build-app-only.bat --environment Sage100

echo.
echo Now SIGN THE EXECUTABLE
echo.
echo. "./sign_s100_app.bat"
echo.
echo. The executable will be signed and saved in the same directory as the original executable.
echo  Then we run the installer build script.
echo.