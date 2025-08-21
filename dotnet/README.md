# Slideshow Web Server (.NET)

A .NET/C# implementation of the slideshow web application that can be run as a standalone application or installed as a Windows service.

## Features

- **Kestrel Web Server**: High-performance HTTP server listening on port 5000
- **Embedded Static Files**: HTML, CSS, and JavaScript files are bundled into the executable
- **Media File Support**: Supports images (JPG, PNG, GIF, WebP, BMP, SVG) and videos (MP4, WebM, OGG, AVI, MOV)
- **Configuration**: JSON-based configuration for slideshow behavior
- **Windows Service**: Can be installed and run as a Windows service
- **Security**: Path traversal protection for media file access
- **Logging**: Comprehensive logging with Event Log support when running as a service

## Projects

### SlideshowWebServer
The main web server application that:
- Serves the slideshow web interface
- Provides API endpoints for configuration and media files
- Streams media files with proper MIME types
- Supports CORS for development

### SlideshowService
A service management utility that can:
- Install the web server as a Windows service
- Uninstall the service
- Start/stop the service
- Check service status

## Quick Start

### 1. Build the Projects

Run the build script:
```batch
build.bat
```

This will create a `publish` folder with the compiled executables.

### 2. Configure the Application

Copy `config.json` to the `publish` folder and edit it:

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

Navigate to the `publish` folder and run:
```batch
SlideshowWebServer.exe
```

The server will start on http://localhost:5000/

### 4. Install as Windows Service

**Important: Run as Administrator**

Navigate to the `publish` folder and run:
```batch
install-and-start.bat
```

Or install and start manually:
```batch
install-service.bat
SlideshowService.exe start
```

## Service Management

All service management commands must be run as Administrator from the `publish` folder:

### Install Service
```batch
SlideshowService.exe install
```

### Start Service
```batch
SlideshowService.exe start
```

### Stop Service
```batch
SlideshowService.exe stop
```

### Check Status
```batch
SlideshowService.exe status
```

### Uninstall Service
```batch
SlideshowService.exe uninstall
```

## Directory Structure

```
dotnet/
├── Slideshow.sln                 # Visual Studio solution file
├── SlideshowWebServer/            # Main web server project
│   ├── SlideshowWebServer.csproj
│   ├── Program.cs                 # Main application entry point
│   ├── Models.cs                  # Data models
│   ├── Services/
│   │   ├── EmbeddedFileProvider.cs
│   │   └── MediaService.cs
│   └── wwwroot/                   # Static web files (embedded)
│       ├── index.html
│       ├── script.js
│       └── styles.css
├── SlideshowService/              # Service management project
│   ├── SlideshowService.csproj
│   └── Program.cs                 # Service installer/manager
├── config.json                    # Sample configuration
├── build.bat                      # Build script
├── install-service.bat            # Service installation script
├── install-and-start.bat          # Install and start script
├── uninstall-service.bat          # Service removal script
└── service-status.bat             # Status check script
```

## API Endpoints

The web server provides the following API endpoints:

- `GET /` - Serves the main slideshow interface
- `GET /api/config` - Returns the current configuration
- `GET /api/files` - Returns list of available media files
- `GET /media/{filename}` - Streams media files with proper MIME types

## Deployment

### For Development
1. Run `build.bat`
2. Copy files from `publish` folder to target machine
3. Edit `config.json` with appropriate `folderPath`
4. Run `SlideshowWebServer.exe`

### For Production (Windows Service)
1. Run `build.bat`
2. Copy files from `publish` folder to target machine (e.g., `C:\Program Files\SlideshowWebServer\`)
3. Edit `config.json` with appropriate `folderPath`
4. Run `install-and-start.bat` as Administrator

## Configuration File Locations

The application searches for `config.json` in the following order:
1. Same directory as the executable
2. Current working directory
3. `./app/config.json` (for compatibility)

## Logging

When running as a standalone application, logs are written to the console.
When running as a Windows service, logs are written to the Windows Event Log under "Application".

## Security Notes

- The application includes path traversal protection to prevent access to files outside the configured media folder
- Only files with supported extensions are served
- The server only listens on HTTP (port 5000) - use a reverse proxy like IIS or nginx for HTTPS in production

## Troubleshooting

### Service Won't Install
- Ensure you're running as Administrator
- Check that SlideshowWebServer.exe exists in the same directory as SlideshowService.exe

### Service Won't Start
- Check Windows Event Log for error messages
- Verify the config.json file is valid JSON
- Ensure the configured folderPath exists and is accessible

### No Media Files Found
- Verify the folderPath in config.json points to the correct directory
- Check that the directory contains files with supported extensions
- Ensure the service has read permissions to the media directory

### Can't Access Web Interface
- Verify the service is running: `SlideshowService.exe status`
- Check that port 5000 is not blocked by Windows Firewall
- Try accessing http://localhost:5000/ directly on the server

## System Requirements

- Windows 10/Windows Server 2016 or later
- .NET 8.0 Runtime (self-contained deployment includes runtime)
- Administrator privileges for service installation
