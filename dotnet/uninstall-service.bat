@echo off
echo Uninstalling Slideshow Web Server Service...
echo.

sc stop SlideshowService
sc delete SlideshowService


echo.
echo Service uninstallation completed.
echo.
