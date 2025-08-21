# Slideshow MAUI App

A cross-platform slideshow application built with .NET MAUI that provides native file reading capabilities and a web-based slideshow interface. The app displays images and videos in a continuous slideshow format with support for fullscreen mode and user controls.

## Features

- **Cross-Platform Support**: Runs on Windows and macOS via Mac Catalyst
- **WebView Integration**: Uses a WebView to display HTML-based slideshow interface
- **Native File Access**: Reads media files directly from the device file system
- **Automatic Operation**: Loads configuration and starts slideshow automatically
- **Media Support**: Displays images (JPG, PNG, GIF, WebP, BMP, SVG) and videos (MP4, WebM, OGG, AVI, MOV)
- **Configurable Settings**: All settings loaded from embedded config.json file
- **Self-Contained**: No external server dependencies

## Project Structure

```
desktop/
├── Services/
│   └── SlideshowService.cs          # Service for handling media files and configuration
├── WebAssets/
│   ├── index.html                   # Main slideshow HTML page
│   ├── styles.css                   # Slideshow styling
│   ├── script.js                    # Slideshow JavaScript for MAUI integration
│   └── README.md                    # Web assets documentation
├── Platforms/                       # Platform-specific configurations
│   ├── Windows/                     # Windows-specific files
│   └── MacCatalyst/                 # macOS Catalyst files
├── Resources/                       # App resources
│   ├── AppIcon/                     # Application icons
│   ├── Fonts/                       # Custom fonts
│   ├── Images/                      # App images
│   ├── Splash/                      # Splash screen
│   └── Styles/                      # XAML styles
├── MainPage.xaml                    # Main UI page definition
├── MainPage.xaml.cs                 # Main page code-behind with WebView logic
├── App.xaml                         # Application definition
├── App.xaml.cs                      # Application code-behind
├── AppShell.xaml                    # Shell navigation (if used)
├── MauiProgram.cs                   # App configuration and service registration
├── SlideshowApp.csproj             # Project file
├── SlideshowApp.sln                # Solution file
└── config.json                     # Configuration file
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

The app loads all configuration from the root `config.json` file:

```json
{
  "imageDuration": 5,
  "folderPath": "/path/to/media/folder",
  "fadeTransitionDuration": 1,
  "autoStart": true,
  "zoomOnImage": true
}
```

**Important**: Update the `folderPath` in `config.json` to point to your media directory before building the app.

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

### Platform-Specific Requirements

#### Windows
- Windows 10 version 1809 or higher
- Visual Studio 2022 with MAUI workload (recommended)

#### macOS
- macOS 10.15 or higher
- Xcode (latest stable version)

### Building

#### For Windows:
```bash
dotnet build -f net8.0-windows10.0.19041.0
```

#### For macOS:
```bash
dotnet build -f net8.0-maccatalyst
```

### Running

#### From Visual Studio:
1. Set the target framework to your desired platform
2. Press F5 or click the Run button

#### From Command Line:
```bash
dotnet run
```

### Publishing

#### Windows:
```bash
dotnet publish -f net8.0-windows10.0.19041.0 -c Release
```

#### macOS:
```bash
dotnet publish -f net8.0-maccatalyst -c Release
```

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
