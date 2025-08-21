@echo off

dotnet publish SlideshowWebServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../../dist
if %ERRORLEVEL% neq 0 (
    echo Failed to build SlideshowWebServer
    pause
    exit /b %ERRORLEVEL%
)
