# Slideshow Application

A web-based slideshow viewer for images and videos with Windows service support.

## Project Structure

```
slideshow/
├── app/                    # Main slideshow application
│   ├── server.js          # Web server
│   ├── index.html         # Slideshow interface
│   ├── script.js          # Client-side logic
│   ├── styles.css         # Styling
│   ├── config.json        # App configuration
│   ├── package.json       # App dependencies
│   └── README.md          # App documentation
└── service/               # Windows service management
    ├── service.js         # Service management script
    ├── package.json       # Service dependencies
    ├── *.bat              # Batch files for easy service management
    └── WINDOWS_SERVICE.md # Windows service documentation
```

## Quick Start

### Running as a Regular Application

1. Install dependencies:
   ```bash
   npm install
   ```

2. Configure your slideshow by editing `app/config.json`

3. Start the application:
   ```bash
   npm start
   ```

4. Open your browser to `http://localhost:3000`

### Running as a Windows Service

For automatic startup and background operation (perfect for digital signage):

1. Install all dependencies (including service dependencies):
   ```bash
   npm run setup-all
   ```

2. Install as Windows service (run as Administrator):
   ```bash
   npm run install-service
   ```

3. Manage the service:
   ```bash
   npm run start-service    # Start the service
   npm run stop-service     # Stop the service
   npm run restart-service  # Restart the service
   npm run uninstall-service # Remove the service
   ```

**Alternative**: You can also use the batch files in the `service/` folder by double-clicking them.

## Features

- **Image & Video Support** - Displays JPG, PNG, GIF, MP4, WebM, and more
- **Configurable Timing** - Set custom durations for images
- **Smooth Transitions** - Fade effects between media
- **Zoom Animation** - Optional Ken Burns effect on images
- **Auto-refresh** - Automatically detects new files in the folder
- **Windows Service** - Runs in background without user login
- **Web-based** - Access from any browser on the network

## Configuration

Edit `app/config.json` to customize your slideshow:

```json
{
  "imageDuration": 5,
  "folderPath": "/path/to/your/media/folder",
  "fadeTransitionDuration": 1,
  "autoStart": true,
  "zoomOnImage": true
}
```

## Documentation

- **App Usage**: See `app/README.md`
- **Windows Service**: See `service/WINDOWS_SERVICE.md`

## Requirements

- Node.js 14.0.0 or higher
- Modern web browser
- Windows OS (for service functionality)
