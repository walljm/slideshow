# Slideshow MAUI App

A cross-platform slideshow application built with .NET MAUI that replaces the Node.js server with native file reading capabilities. The app displays images and videos in a continuous slideshow format with support for fullscreen mode and user controls.

## Features

- **Cross-Platform Support**: Runs on Windows and macOS
- **WebView Integration**: Uses a WebView to display HTML-based slideshow
- **Native File Access**: Reads media files directly from the device file system
- **Automatic Operation**: Loads configuration and starts slideshow automatically
- **Media Support**: Displays images (JPG, PNG, GIF, WebP, BMP, SVG) and videos (MP4, WebM, OGG, AVI, MOV)
- **Configurable Settings**: All settings loaded from config.json file

## Project Structure

```
/
├── Services/
│   └── SlideshowService.cs          # Service for handling media files and configuration
├── WebAssets/
│   ├── index.html                   # Main slideshow HTML page
│   ├── styles.css                   # Slideshow styling
│   ├── script.js                    # Modified slideshow JavaScript for MAUI
│   └── config.json                  # Configuration file
├── MainPage.xaml                    # Main UI page
├── MainPage.xaml.cs                 # Main page code-behind with WebView logic
├── App.xaml                         # Application definition
├── MauiProgram.cs                   # App configuration and service registration
└── SlideshowApp.csproj             # Project file
```

## Architecture

The application follows a simplified automatic approach:

1. **Native MAUI UI**: Minimal interface with just the WebView
2. **WebView Component**: Hosts the HTML/CSS/JavaScript slideshow interface
3. **Service Layer**: `SlideshowService` handles file system operations and configuration
4. **Automatic Configuration**: All settings loaded from bundled `config.json` file
5. **Bridge Communication**: Uses JavaScript evaluation to communicate between MAUI and WebView

## Key Components

### SlideshowService
- Manages media file discovery and loading
- Handles configuration persistence
- Converts files to base64 data URLs for WebView consumption
- Provides filtering for supported file types

### MainPage
- Hosts the WebView component with automatic initialization
- Minimal UI with no visible controls
- Handles communication between MAUI and JavaScript
- Automatically loads and starts slideshow on launch

### WebView Integration
- Loads HTML content with embedded CSS and JavaScript
- Intercepts API calls using navigation events
- Provides slideshow functionality with fade transitions and zoom effects

## Configuration

The app loads all configuration from the bundled `WebAssets/config.json` file:

```json
{
  "imageDuration": 5,
  "folderPath": "/path/to/media/folder",
  "fadeTransitionDuration": 1,
  "autoStart": true,
  "zoomOnImage": true
}
```

**Important**: Update the `folderPath` in `WebAssets/config.json` to point to your media directory before building the app.

## Operation

The app operates automatically:
1. Loads configuration from `config.json`
2. Scans the specified folder for media files
3. Automatically starts the slideshow
4. Continuously cycles through media files
5. If no files are found, retries every 5 seconds

## Platform-Specific Features

### Windows
- Native folder picker integration
- Full keyboard support

### macOS
- Native folder picker via Mac Catalyst
- Optimized for macOS interface guidelines

## Building and Running

### Prerequisites
- .NET 8.0 SDK
- .NET MAUI workload: `dotnet workload install maui`

### Build
```bash
dotnet build
```

### Run
```bash
# For macOS
dotnet run --framework net8.0-maccatalyst

# For Windows
dotnet run --framework net8.0-windows10.0.19041.0
```

### VS Code
Use the configured task "Run Slideshow App" or press `Ctrl+Shift+P` and select "Tasks: Run Task"

## Development Notes

### WebView Communication
The app uses a custom approach for WebView communication:
1. JavaScript triggers navigation to API endpoints
2. MAUI intercepts these navigations
3. MAUI processes requests and injects responses via JavaScript evaluation
4. Slideshow starts automatically once configuration and files are loaded

### File Handling
Media files are converted to base64 data URLs to work within the WebView security context. This approach ensures cross-platform compatibility but may use more memory for large files.

### Performance Considerations
- Base64 encoding increases memory usage
- Large media collections may impact performance
- Consider implementing pagination for very large folders

## Future Enhancements

- [ ] Add thumbnail generation for faster loading
- [ ] Implement streaming for large video files
- [ ] Add support for nested folder scanning
- [ ] Include metadata display (EXIF data, file info)
- [ ] Add playlist and favorites functionality
- [ ] Implement background music support

## License

This project is part of a slideshow application suite that replaces a Node.js server with native MAUI capabilities.
