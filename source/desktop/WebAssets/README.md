# Slideshow Web Assets

Web-based slideshow interface assets used by both the MAUI desktop application and the .NET web server. This folder contains the HTML, CSS, and JavaScript files that provide the slideshow functionality.

## Features

- 📁 Displays images and videos from configured media sources
- ⏱️ Configurable display duration for images
- 🎬 Auto-play videos with natural duration
- 🌅 Smooth fade transitions between media
- 🔄 Automatic loop through media files
- 🎯 Clean, control-free interface
- 📱 Responsive design
- 🔧 Cross-platform compatibility (MAUI and Web Server)

## Supported File Types

**Images:** JPG, JPEG, PNG, GIF, WebP, BMP, SVG  
**Videos:** MP4, WebM, OGG, AVI, MOV

## Usage Context

These web assets are used in two different contexts:

### 1. MAUI Desktop Application
- Files are embedded as resources in the MAUI app
- JavaScript communicates with native MAUI code via WebView bridges
- Media files are loaded from the local file system via `SlideshowService`
- Configuration is loaded from the app's `config.json` file

### 2. .NET Web Server
- Files are embedded as resources in the web server executable
- JavaScript communicates with the server via HTTP API endpoints
- Media files are served through HTTP endpoints
- Configuration is served via `/api/config` endpoint

## Configuration

The slideshow behavior is controlled by configuration that varies by usage context:

```json
{
  "imageDuration": 5,
  "folderPath": "/path/to/media/folder",
  "fadeTransitionDuration": 1,
  "autoStart": true,
  "zoomOnImage": true,
  "supportedImageExtensions": ["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"],
  "supportedVideoExtensions": ["mp4", "webm", "ogg", "avi", "mov"]
}
```

### Configuration Options

- **imageDuration**: Seconds to display each image (videos use their natural duration)
- **folderPath**: Path to the folder containing media files
- **fadeTransitionDuration**: Duration of fade transition in seconds
- **autoStart**: Whether to start the slideshow automatically
- **zoomOnImage**: Whether to apply subtle zoom animation to images
- **supportedImageExtensions**: Array of supported image file extensions
- **supportedVideoExtensions**: Array of supported video file extensions

## File Structure

```
WebAssets/
├── index.html          # Main slideshow HTML page
├── script.js           # Slideshow logic and bridge communication
├── styles.css          # Slideshow styling and animations
└── README.md           # This documentation
```

## How It Works

### General Flow
1. Configuration is loaded (method varies by context)
2. Media files are discovered and loaded
3. Files are sorted and prepared for display
4. Images display for the configured duration
5. Videos play automatically for their natural duration
6. Smooth fade transitions occur between all media
7. After the last file, the slideshow loops back to the beginning

### MAUI Integration
- Uses `window.chrome.webview.postMessage()` for MAUI communication
- Receives media data as base64-encoded data URLs
- Configuration loaded via native bridge

### Web Server Integration
- Uses standard HTTP API calls (`fetch()`)
- Media files served via HTTP endpoints
- Configuration loaded via `/api/config`

## API Interface

### For Web Server Context
- `GET /api/config` - Returns the current configuration
- `GET /api/files` - Returns the list of media files
- `GET /media/[filename]` - Serves individual media files

### For MAUI Context
- Native bridge communication for configuration and file data
- Media files provided as base64 data URLs
- No HTTP endpoints required

## Browser Compatibility

Works with all modern browsers and WebView controls:
- Chrome/Chromium ✅
- Firefox ✅
- Safari ✅
- Edge ✅
- WebView2 (MAUI Windows) ✅
- WKWebView (MAUI macOS) ✅

## Technical Details

- **Dual Container System**: Uses two containers for smooth crossfading effects
- **Automatic Error Handling**: Gracefully handles corrupted/missing files
- **Responsive Design**: Adapts to different screen sizes and orientations
- **Performance Optimized**: Efficient memory usage and smooth animations
- **Cross-Platform Bridge**: Supports both HTTP API and native MAUI communication

## Security

- **Path Validation**: Media files are validated for security (web server context)
- **CORS Support**: Proper CORS headers for cross-origin requests (web server context)
- **Sandboxed Execution**: Runs within WebView sandbox (MAUI context)
- **No File System Access**: Web assets cannot directly access the file system
- Responsive design adapts to any screen size

Enjoy your slideshow! 🎉
