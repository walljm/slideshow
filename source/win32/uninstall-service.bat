@echo off
echo Uninstalling Slideshow Web Server Service...
echo.
echo This script must be run as Administrator!
echo.

:: Check if service exists first
sc query SlideshowService >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Service 'SlideshowService' not found. It may already be uninstalled.
    echo.
    pause
    exit /b 0
)

:: Stop the service if it's running
echo Stopping service...
sc stop SlideshowService
if %ERRORLEVEL% equ 0 (
    echo Service stopped successfully.
) else (
    echo Service was not running or failed to stop.
)

:: Wait a moment for service to fully stop
timeout /t 3 /nobreak >nul

:: Delete the service
echo Deleting service...
sc delete SlideshowService

if %ERRORLEVEL% equ 0 (
    echo Service deleted successfully.
) else (
    echo Failed to delete service. Error code: %ERRORLEVEL%
    echo You may need to restart the computer and try again.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Service uninstallation completed successfully!
echo The Slideshow Web Server service has been removed from the system.
echo.
pause
