using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlideshowWebServer;
using System.Runtime.InteropServices;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure System.Text.Json for polymorphism
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.AllowTrailingCommas = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.TypeInfoResolver = JsonContext.Default;
});

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