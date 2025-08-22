# Slideshow Web Server (.NET)

A .NET slideshow web application that can be configured to display images and videos in a web browser. It supports both standalone execution and installation as a Windows service.

## Features

- **Media File Support**: Supports images (JPG, PNG, GIF, WebP, BMP, SVG) and videos (MP4, WebM, OGG, AVI, MOV)
- **Configuration**: JSON-based configuration for slideshow behavior
- **Windows Service**: Can be installed and run as a Windows service
- **Single File Deployment**: Self-contained executable with all dependencies


# 2. Configure the Application

Edit the `config.json` in the `dist` folder:

```json
{
  "imageDuration": 5,
  "folderPath": "C:\\Path\\To\\Your\\Media\\Files",
  "fadeTransitionDuration": 1,
  "autoStart": true,
  "zoomOnImage": true
}
```

**Configuration Options:**
- `imageDuration`: How long to display each image (in seconds)
- `folderPath`: Full path to the folder containing your media files
- `fadeTransitionDuration`: Duration of fade transition between images/videos (in seconds)
- `zoomOnImage`: Whether to apply subtle zoom animation to images

### Configuration File Locations

The application searches for `config.json` in the following order:
1. Same directory as the executable
2. Current working directory

## Logging

When running as a standalone application, logs are written to the console.
When running as a Windows service, logs are written to the Windows Event Log under "Application".

## Security Notes

- The application includes path traversal protection to prevent access to files outside the configured media folder
- Only files with supported extensions are served
- The server listens on HTTP (default port 5500) - use a reverse proxy like IIS or nginx for HTTPS in production
- When changing URLs via command line, ensure the new ports don't conflict with other services

# 3. Run as Standalone Application

## Platform-Specific Executables

The application can be built for multiple platforms. Navigate to the `dist` folder and run the appropriate executable for your platform:

### Windows (x64)
```batch
SlideshowWebServer.exe
```

### Linux (x64), macOS (Intel x64, Apple Silicon ARM)
```bash
./SlideshowWebServer
```

**Note**: On macOS and Linux, you may need to make the executable file executable first:
```bash
chmod +x SlideshowWebServer
```

## Building from Source

To build the application for different platforms, use the provided build scripts:

### Windows Build
```batch
buildwin.bat
```

### Linux x64 Build
```bash
./buildlinuxx64.sh
```

### macOS Intel x64 Build
```bash
./buildmacx64.sh
```

### macOS Apple Silicon ARM Build
```bash
./buildmacarm.sh
```

All builds will output to the `./dist` folder as a self-contained executable with all dependencies included.

The server will start on http://localhost:5500/ by default.

## Command Line Options

The application supports various command line arguments for configuration:

### URL Configuration

You can change the URLs and ports the server listens on using the `--Urls` parameter:

```batch
# Listen on a different port
SlideshowWebServer.exe --urls "http://localhost:8080"

# Listen on all interfaces on port 5500 (default)
SlideshowWebServer.exe --urls "http://*:5500"

# Listen on specific IP address
SlideshowWebServer.exe --urls "http://192.168.1.100:5500"

# Listen on multiple URLs
SlideshowWebServer.exe --urls "http://localhost:5500;https://localhost:5501"

# Listen on all interfaces with a custom port
SlideshowWebServer.exe --urls "http://*:3000"
```

### Other Configuration Options

You can override any configuration setting using command line arguments:

```batch
# Override logging level
SlideshowWebServer.exe --Logging:LogLevel:Default=Debug

# Combine URL and logging configuration
SlideshowWebServer.exe --urls "http://*:8080" --Logging:LogLevel:Default=Debug
```

# 4. Install as Windows Service

**Important: Run as Administrator**

## Service Management

All service management commands must be run as Administrator from the `dist/win32` folder:

### Install Service
```batch
install-service.bat
```

The install script will:
- Create Windows Event Log sources for proper logging
- Create the service with the name "SlideshowService" and display name "Slideshow Web Server"
- Set the service to start automatically on system boot
- Start the service immediately after creation

**Service Details:**
- **Service Name**: SlideshowService
- **Display Name**: Slideshow Web Server
- **Description**: Web-based slideshow application that displays images and videos from a configured folder. Accessible via web browser.
- **Start Type**: Automatic
- **Run As**: NT AUTHORITY\NetworkService

### Uninstall Service
```batch
uninstall-service.bat
```
