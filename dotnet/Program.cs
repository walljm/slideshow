using System.Runtime.InteropServices;
using SlideshowWebServer;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<MediaService>();
builder.Services.AddSingleton<EmbeddedFileProvider>();


// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Windows service support
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddWindowsService();
    builder.Logging.AddEventLog();
}

var app = builder.Build();

// API endpoints
var api = app.MapGroup("/api");
api.MapGet("config", static (MediaService mediaService) => Results.Json(mediaService.Config));
api.MapGet("files", static (MediaService mediaService) => Results.Json(mediaService.GetMediaFiles()));

// Media file serving
app.MapGet(
    "/media/{fileName}",
    static async (string fileName, MediaService mediaService) =>
    {
        var (stream, contentType) = await mediaService.GetMediaFileStreamAsync(fileName);

        if (stream == null)
        {
            return Results.NotFound("File not found");
        }

        return Results.Stream(stream, contentType, fileName, DateTimeOffset.UtcNow, enableRangeProcessing: true);
    }
);

// Static file serving from embedded resources
app.MapGet(
    "/{*path}",
    static async (string? path, EmbeddedFileProvider fileProvider) =>
    {
        // Default to index.html
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            path = "index.html";
        }

        try
        {
            var (content, contentType) = await fileProvider.GetFileAsync(path);
            return Results.Bytes(content, contentType);
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound($"File '{path}' not found");
        }
    }
);


const int port = 5500;
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"Slideshow server starting on http://localhost:{port}/");
app.Run($"http://+:{port}");