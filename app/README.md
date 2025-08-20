# Slideshow Viewer

A web-based slideshow application that displays images and videos from a configured local folder with smooth transitions and configurable timing.

## Features

- 📁 Automatically loads images and videos from a configured folder
- ⏱️ Configurable display duration for images via config file
- 🎬 Auto-play videos with natural duration
- 🌅 Smooth fade transitions between media
- 🔄 Automatic loop - rescans folder after reaching the end
- 🎯 Clean, control-free interface
- 📱 Responsive design
- �️ Server-side configuration

## Supported File Types

**Images:** JPG, JPEG, PNG, GIF, WebP, BMP, SVG  
**Videos:** MP4, WebM, OGG, AVI, MOV

## Configuration

Edit `config.json` to customize the slideshow:

```json
{
  "imageDuration": 5,
  "folderPath": "/Users/walljm/Pictures",
  "fadeTransitionDuration": 1,
  "autoStart": true,
  "supportedImageExtensions": ["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"],
  "supportedVideoExtensions": ["mp4", "webm", "ogg", "avi", "mov"]
}
```

### Configuration Options

- **imageDuration**: Seconds to display each image (videos use their natural duration)
- **folderPath**: Absolute path to the folder containing your media files
- **fadeTransitionDuration**: Duration of fade transition in seconds (CSS)
- **autoStart**: Whether to start the slideshow automatically
- **supportedImageExtensions**: Array of supported image file extensions
- **supportedVideoExtensions**: Array of supported video file extensions

## Quick Start

1. **Configure the slideshow:**
   Edit `config.json` and set the `folderPath` to your media directory

2. **Start the server:**
   ```bash
   npm start
   ```
   or
   ```bash
   node server.js
   ```

3. **Open your browser:**
   Go to `http://localhost:3000`

4. **Enjoy the slideshow!**
   The slideshow will start automatically if media files are found

## How It Works

1. The server reads configuration from `config.json`
2. Media files are loaded from the configured folder path
3. Files are sorted alphabetically and served via HTTP endpoints
4. Images display for the configured duration
5. Videos play automatically for their natural duration
6. Smooth fade transitions occur between all media
7. After the last file, the folder is rescanned and the slideshow loops

## API Endpoints

- `GET /api/config` - Returns the current configuration
- `GET /api/files` - Returns the list of media files
- `GET /media/[filename]` - Serves individual media files

## Browser Compatibility

Works with all modern browsers - no special APIs required:
- Chrome ✅
- Firefox ✅
- Safari ✅
- Edge ✅

## Security

- Media files are served securely with path validation
- Only files within the configured folder can be accessed
- No file system browsing capabilities exposed

## Technical Details

- Pure server-side file management - no client-side file system access
- Uses dual container system for smooth crossfading
- Automatic error handling for corrupted/missing files
- RESTful API design for configuration and media serving
- Responsive design adapts to any screen size

Enjoy your slideshow! 🎉
