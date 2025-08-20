@echo off
echo Installing Slideshow Server as Windows Service...
cd /d "%~dp0.."
npm run install-service
pause
