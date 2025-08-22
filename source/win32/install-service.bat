@echo off
echo Installing and Starting Slideshow Web Server Service...
echo.
echo This script must be run as Administrator!
echo.

:: Check if we're running from the correct directory (should be in the dist folder)
if not exist "..\SlideshowWebServer.exe" (
    echo Error: SlideshowWebServer.exe not found in parent directory.
    echo Please ensure this script is run from the win32 folder inside the dist folder.
    echo Expected structure: dist\SlideshowWebServer.exe and dist\win32\install-service.bat
    pause
    exit /b 1
)

:: Get the full path to the executable (parent directory of win32 folder)
for %%i in ("%~dp0..") do set "EXEC_PATH=%%~fi\SlideshowWebServer.exe"
echo Service executable path: %EXEC_PATH%

:: Create the event source(s) explicitly. The Network Service account is not
:: granted permission to create new event sources implicitly when the service
:: is started. See the channel access for more details:
::
::   wevtutil get-log Application
::
echo Creating event sources...
eventcreate /ID 1 /L APPLICATION /T INFORMATION /SO SlideshowService /D "Created the service event source." 2>nul
eventcreate /ID 1 /L APPLICATION /T INFORMATION /SO SlideshowWebServer /D "Created the application event source." 2>nul

echo Creating service...
sc create SlideshowService ^
  binpath= "\"%EXEC_PATH%\"" ^
  displayName= "Slideshow Web Server" ^
  description= "Web-based slideshow application that displays images and videos from a configured folder. Accessible via web browser." ^
  start= "auto" ^
  obj= "NT AUTHORITY\NetworkService"

if %ERRORLEVEL% neq 0 (
    echo Failed to create service. Error code: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo Starting service...
sc start SlideshowService

if %ERRORLEVEL% neq 0 (
    echo Failed to start service. Error code: %ERRORLEVEL%
    echo You may need to start it manually or check the Windows Event Log.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Service installation and startup completed successfully!
echo Service Name: SlideshowService
echo Display Name: Slideshow Web Server
echo Executable: %EXEC_PATH%
echo.
echo The service will start automatically on system boot.
echo To check service status: sc query SlideshowService
echo To stop service: sc stop SlideshowService
echo.
pause
