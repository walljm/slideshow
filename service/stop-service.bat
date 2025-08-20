@echo off
echo Stopping Slideshow Server Service...
cd /d "%~dp0.."
npm run stop-service
pause
