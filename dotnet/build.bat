@echo off
echo Building Slideshow Web Server and Service...
echo.

dotnet publish SlideshowWebServer\SlideshowWebServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
if %ERRORLEVEL% neq 0 (
    echo Failed to build SlideshowWebServer
    pause
    exit /b %ERRORLEVEL%
)

dotnet publish SlideshowService\SlideshowService.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
if %ERRORLEVEL% neq 0 (
    echo Failed to build SlideshowService
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
echo.
echo Files are available in the 'publish' folder:
echo - SlideshowWebServer.exe (the web server)
echo - SlideshowService.exe (service manager)
echo.
echo Copy config.json to the publish folder and customize the folderPath setting.
echo.
pause
