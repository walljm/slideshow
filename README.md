# Slideshow Web Server

A .NET slideshow web application that can be configured to display images and videos in a web browser. It supports both standalone execution and installation as a Windows service.

## Features

- **Media File Support**: Supports images (JPG, PNG, GIF, WebP, BMP, SVG) and videos (MP4, WebM, OGG, AVI, MOV)
- **Configuration**: JSON-based configuration for slideshow behavior
- **Windows Service**: Can be installed and run as a Windows service
- **Single File Deployment**: Self-contained executable with all dependencies

## Configuration

Edit the `config.json` in the `dist` folder:

```json
{
  "imageDuration": 5,
  "folderPath": "C:\\Path\\To\\Your\\Media\\Files",
  "fadeTransitionDuration": 1,
  "zoomOnImage": true,
  "displayOrder": "Alpha"
}
```

**Configuration Options:**
- `imageDuration`: How long to display each image (in seconds)
- `folderPath`: Full path to the folder containing your media files
- `fadeTransitionDuration`: Duration of fade transition between images/videos (in seconds)
- `zoomOnImage`: Whether to apply subtle zoom animation to images
- `displayOrder`: Order to display media files - `"Alpha"` for alphabetical, `"Random"` for random order

### Configuration File Locations

The application searches for `config.json` in the following order:
1. Same directory as the executable
2. Current working directory

### Logging

When running as a standalone application, logs are written to the console.
When running as a Windows service, logs are written to the Windows Event Log under "Application".

### Security Notes

- The application includes path traversal protection to prevent access to files outside the configured media folder
- Only files with supported extensions are served
- The server listens on HTTP (default port 5500) - use a reverse proxy like IIS or nginx for HTTPS in production
- When changing URLs via command line, ensure the new ports don't conflict with other services

## Running the Application

### Standalone Application

#### Platform-Specific Executables

The application can be built for multiple platforms. Navigate to the `dist` folder and run the appropriate executable for your platform:

##### Windows (x64)
```batch
SlideshowWebServer.exe
```

##### Linux (x64), macOS (Intel x64, Apple Silicon ARM)
```bash
./SlideshowWebServer
```

**Note**: On macOS and Linux, you may need to make the executable file executable first:
```bash
chmod +x SlideshowWebServer
```

#### Building from Source

To build the application for different platforms, use the provided build scripts:

##### Windows Build
```batch
buildwin.bat
```

##### Linux x64 Build
```bash
./buildlinuxx64.sh
```

##### macOS Intel x64 Build
```bash
./buildmacx64.sh
```

##### macOS Apple Silicon ARM Build
```bash
./buildmacarm.sh
```

All builds will output to the `./dist` folder as a self-contained executable with all dependencies included.

The server will start on http://localhost:5500/ by default.

#### Command Line Options

The application supports various command line arguments for configuration:

##### URL Configuration

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

##### Other Configuration Options

You can override any configuration setting using command line arguments:

```batch
# Override logging level
SlideshowWebServer.exe --Logging:LogLevel:Default=Debug

# Combine URL and logging configuration
SlideshowWebServer.exe --urls "http://*:8080" --Logging:LogLevel:Default=Debug
```

### Windows Service Installation

**Important: Run as Administrator**

#### Service Management

All service management commands must be run as Administrator from the `dist/win32` folder:

##### Install Service
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
- **Run As**: Local System (for broad file system access)

##### Uninstall Service
```batch
uninstall-service.bat
```

#### Security and Service Account Configuration

##### Default Service Account

By default, the Windows service runs as **Local System**, which provides broad access to the file system and eliminates most media folder access issues. This account can access:
- All local drives and folders
- Network resources (if the computer account has permissions)
- User profile folders (Documents, Pictures, etc.)

##### When to Change the Service Account

While Local System provides the best compatibility, you may want to use a more restrictive account in security-sensitive environments:

**Consider changing the service account if:**
- The system handles sensitive data or is on a corporate network
- You need to comply with security policies that require least-privilege service accounts
- The slideshow will only access a specific folder that can be secured for a dedicated account

##### How to Change Service Account

**Use Services Management Console (Recommended)**

1. **Open Services Management:**
   - Press `Win + R`, type `services.msc`, and press Enter
   - Or search for "Services" in the Start menu

2. **Find and Configure the Service:**
   - Locate "Slideshow Web Server" in the services list
   - Right-click on it and select "Properties"
   - Go to the "Log On" tab

3. **Change the User Account:**
   - Current default is "Local System account"
   - To use a more restrictive account, select "This account"
   - **For NetworkService:** Enter `NT AUTHORITY\NetworkService` (no password required)
   - **For dedicated service account:** Use `DOMAIN\ServiceAccount` or `.\LocalAccount`
   - **For specific user:** Use `DOMAIN\Username` or `COMPUTERNAME\Username`
   - Enter the password if required
   - Click "OK"

4. **Restart the Service:**
   - In the Services console, right-click "Slideshow Web Server" and select "Restart"
   - Or use Command Prompt as Administrator:
     ```batch
     sc stop SlideshowService
     sc start SlideshowService
     ```

```

##### Security Considerations

**Local System Account (Default):**
- **Pros:** Maximum compatibility, can access any local folder or network resource
- **Cons:** Has full administrative privileges on the system
- **Best for:** Dedicated kiosk machines, home media servers, isolated display systems

**NetworkService Account:**
- **Pros:** More secure, limited system privileges
- **Cons:** May not access user folders, network drives, or mapped drives without additional configuration
- **Best for:** Corporate environments, systems with strict security requirements

**Dedicated Service Account:**
- **Pros:** Can be granted only the specific permissions needed
- **Cons:** Requires manual setup and permission management
- **Best for:** Enterprise deployments with specific security policies

**General Security Tips:**
- Use Local System on dedicated/isolated systems where convenience matters most
- Use NetworkService or dedicated accounts in corporate/shared environments
- Ensure the media folder has appropriate access controls regardless of service account
- Consider network-level security (firewall rules) to limit access to the web interface

##### Verifying Access

After changing the service user:

1. **Check service status:**
   ```batch
   sc query SlideshowService
   ```

2. **Check the Windows Event Log** for any access errors:
   - Open Event Viewer (`eventvwr.msc`)
   - Navigate to Windows Logs > Application
   - Look for events from "SlideshowService" or "SlideshowWebServer"

3. **Test the web interface** by accessing the slideshow at `http://localhost:5500`

##### Troubleshooting Access Issues

If you change from Local System to a more restrictive account and encounter access issues:

1. **Verify folder permissions:** Ensure the service account has read access to the media folder
2. **Check for mapped drives:** NetworkService cannot access mapped drives - use UNC paths instead
3. **Review Event Log:** Check Windows Event Viewer for specific error messages
4. **Test with Local System:** Temporarily switch back to Local System to confirm the issue is account-related

**For NetworkService specifically:**
- Cannot access user profile folders (Documents, Desktop, etc.)
- Cannot access mapped network drives (use UNC paths like `\\server\share\folder`)
- May need "Log on as a service" rights for custom accounts
