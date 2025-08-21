@echo off
echo Building Slideshow Web Server and Service...
echo.

dotnet publish SlideshowWebServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../publish
if %ERRORLEVEL% neq 0 (
    echo Failed to build SlideshowWebServer
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
echo.