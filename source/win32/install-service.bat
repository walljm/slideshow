@echo off
echo Installing and Starting Slideshow Web Server Service...
echo.
echo This script must be run as Administrator!
echo.

if not exist "SlideshowWebServer.exe" (
    echo Error: SlideshowWebServer.exe not found in current directory.
    echo Please run build.bat first and copy the files from the publish folder.
    pause
    exit /b 1
)

:: Create the event source(s) explicitly. The Network Service account is not
:: granted permission to create new event sources implicitly when the service
:: is started. See the channel access for more details:
::
::   wevtutil get-log Application
::
eventcreate /ID 1 /L APPLICATION /T INFORMATION /SO SlideshowService /D "Created the service event source."
eventcreate /ID 1 /L APPLICATION /T INFORMATION /SO SlideshowWebServer /D "Created the application event source."

sc create SlideshowService ^
  binpath= "SlideshowWebServer.exe" ^
  displayName= "Slideshow Web Server" ^
  start= "auto" ^
  obj= "NT AUTHORITY\NetworkService"
