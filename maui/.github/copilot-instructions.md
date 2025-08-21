<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Slideshow MAUI App Development Guidelines

This is a .NET MAUI application that creates a cross-platform slideshow viewer for Windows and macOS. The app replaces a Node.js server with native file reading capabilities and uses a WebView to display HTML-based slideshows. The application operates automatically without user controls, loading all configuration from a bundled config.json file.

## Project Checklist - COMPLETED ✅

- [x] Verify that the copilot-instructions.md file in the .github directory is created.
- [x] Clarify Project Requirements - .NET MAUI slideshow application with WebView, file reading, and cross-platform support
- [x] Scaffold the Project - .NET MAUI project created successfully with dotnet new maui
- [x] Customize the Project - Implementing slideshow functionality with WebView, file reading, and cross-platform support
- [x] Install Required Extensions - No special extensions required for .NET MAUI
- [x] Compile the Project - Project builds successfully without warnings
- [x] Create and Run Task - Task created for running the slideshow app
- [x] Launch the Project - Project is ready to run using the task system
- [x] Ensure Documentation is Complete - README.md created with comprehensive project documentation

## Architecture Overview

- **Minimal MAUI UI**: Clean interface with only WebView component
- **WebView Component**: HTML/CSS/JavaScript slideshow interface  
- **Service Layer**: File system operations and configuration management
- **Automatic Operation**: Configuration loaded from bundled config.json, no user controls
- **Bridge Communication**: JavaScript evaluation for MAUI-WebView communication

## Key Files

- `Services/SlideshowService.cs`: Media file handling and configuration
- `MainPage.xaml/xaml.cs`: Main UI and WebView integration
- `WebAssets/`: HTML slideshow assets (index.html, styles.css, script.js)
- `README.md`: Comprehensive documentation

## Development Commands

- Build: `dotnet build`
- Run: `dotnet run --framework net8.0-maccatalyst` (macOS) or `net8.0-windows10.0.19041.0` (Windows)
- Use VS Code task: "Run Slideshow App"
