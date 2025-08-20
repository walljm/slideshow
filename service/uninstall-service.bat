@echo off
echo Uninstalling Slideshow Server Windows Service...
cd /d "%~dp0.."
npm run uninstall-service
pause
