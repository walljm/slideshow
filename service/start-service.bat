@echo off
echo Starting Slideshow Server Service...
cd /d "%~dp0.."
npm run start-service
pause
