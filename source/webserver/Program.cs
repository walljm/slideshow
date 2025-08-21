using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

app.AddSlideshowEndpoints();

const int port = 5500;
app.Services
   .GetRequiredService<ILogger<Program>>()
   .LogInformation("Slideshow server starting on http://localhost:{Port}/", port);
app.Run($"http://+:{port}");