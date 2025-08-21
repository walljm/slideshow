@echo off
echo Uninstalling Slideshow Web Server Service...
echo.
echo This script must be run as Administrator!
echo.

sc stop SlideshowService
sc delete SlideshowService


echo.
echo Service uninstallation completed.
echo.
pause
