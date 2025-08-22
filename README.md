# Slideshow Web Server (.NET)

A .NET/C# implementation of the slideshow web application that can be run as a standalone application or installed as a Windows service.

## Features

- **Kestrel Web Server**: High-performance HTTP server listening on port 5500
- **Embedded Static Files**: HTML, CSS, and JavaScript files are bundled into the executable
- **Media File Support**: Supports images (JPG, PNG, GIF, WebP, BMP, SVG) and videos (MP4, WebM, OGG, AVI, MOV)
- **Configuration**: JSON-based configuration for slideshow behavior
- **Windows Service**: Can be installed and run as a Windows service
- **Security**: Path traversal protection for media file access
- **Logging**: Comprehensive logging with Event Log support when running as a service
- **Single File Deployment**: Self-contained executable with all dependencies

## Architecture

The application consists of a single .NET project (`SlideshowWebServer`) that:
- Serves the slideshow web interface from embedded resources
- Provides API endpoints for configuration and media files
- Streams media files with proper MIME types
- Supports both standalone and Windows service execution modes
- Uses the `MediaService` for file operations and `EmbeddedFileProvider` for static content

## Quick Start

### 2. Configure the Application

Copy `config.json` to the `dist` folder and edit it:

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
- `fadeTransitionDuration`: Duration of fade transition between images (in seconds)
- `autoStart`: Whether to start the slideshow automatically when loaded
- `zoomOnImage`: Whether to apply subtle zoom animation to images

### 3. Run as Standalone Application

Navigate to the `dist` folder and run:
```batch
SlideshowWebServer.exe
```

The server will start on http://localhost:5500/

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

### 4. Install as Windows Service

**Important: Run as Administrator**

## Service Management

All service management commands must be run as Administrator from the `dist/win32` folder:

### Install Service
```batch
install-service.bat
```

**Note**: To configure the service to run on a different port, you can modify the service after installation or create a custom service configuration. The service will use the default URL configuration (http://*:5500) unless overridden through environment variables.

### Uninstall Service
```batch
uninstall-service.bat
```

### Prerequisites
- .NET 8.0 SDK
- Windows 10/11 or Windows Server 2019+ (for service functionality)

## Configuration File Locations

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
