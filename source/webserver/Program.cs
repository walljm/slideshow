using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlideshowWebServer;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

const int port = 5500;

var builder = WebApplication.CreateBuilder(args);

// Configure System.Text.Json for polymorphism
builder.Services.Configure<JsonOptions>(static options =>
{
    options.SerializerOptions.AllowTrailingCommas = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.TypeInfoResolver = JsonContext.Default;
});

// Add services
builder.Services.AddSingleton<MediaService>();
builder.Services.AddSingleton<EmbeddedFileProvider>();

var productionConfigurationDefaults = new Dictionary<string, string?>
{
    ["AllowedHosts"] = "*",
    ["Urls"] = $"http://*:{port}",
    // Default logging (EventLog and Console)
    ["Logging:LogLevel:Default"] = "Information",
    ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information",
    ["Logging:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics"] = "Warning",
    ["Logging:LogLevel:Microsoft.AspNetCore.Routing.EndpointMiddleware"] = "Warning",
    // Debug logger.
    ["Logging:Debug:LogLevel:Default"] = "Debug",
};
builder.Configuration.AddInMemoryCollection(productionConfigurationDefaults);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Windows service support
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddWindowsService();
    builder.Logging.AddEventLog();
}

var app = builder.Build();

app.AddSlideshowEndpoints();

app.Services
   .GetRequiredService<ILogger<Program>>()
   .LogInformation("Slideshow server starting on http://*:{Port}/", port);
app.Run();