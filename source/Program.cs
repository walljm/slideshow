using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using System.Runtime.InteropServices;
using System.Text.Json;

using SlideshowWebServer;

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
    ["Urls"] = "http://*:5500",
    // Default logging (EventLog and Console)
    ["Logging:LogLevel:Default"] = "Warning",
    ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information",
    // Debug logger.
    ["Logging:Debug:LogLevel:Default"] = "Debug",
};
builder.Configuration.AddInMemoryCollection(productionConfigurationDefaults);
builder.Configuration.AddCommandLine(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Windows service support
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddWindowsService();
    builder.Logging.AddEventLog(options =>
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            options.SourceName = "SlideshowService";
            options.LogName = "Application";
        }
    });
}

// Add linux service support
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.Host.UseSystemd();
}

var app = builder.Build();
app.AddSlideshowEndpoints();
app.Run();