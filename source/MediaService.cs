using System.Text.Json;
using Microsoft.Extensions.Logging;
using SlideshowWebServer.Models;

namespace SlideshowWebServer;

public sealed class MediaService
{
    private static readonly string[] SupportedImageExtensions = ["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"];

    private static readonly string[] SupportedVideoExtensions = ["mp4", "webm", "ogg", "avi", "mov"];

    private static readonly string[] AllSupportedExtensions = [.. SupportedImageExtensions, .. SupportedVideoExtensions];

    // Check multiple possible locations for config.json
    private static readonly string[] PossibleConfigPaths =
    [
        Path.Combine(AppContext.BaseDirectory, "config.json"),
        Path.Combine(Environment.CurrentDirectory, "config.json"),
    ];

    private readonly ILogger<MediaService> logger;
    public SlideshowConfig Config { get;  }

    public MediaService(ILogger<MediaService> logger)
    {
        this.logger = logger;
        this.Config = LoadConfiguration();
    }

    private SlideshowConfig LoadConfiguration()
    {
        foreach (var configPath in PossibleConfigPaths)
        {
            if (File.Exists(configPath))
            {
                var configData = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<SlideshowConfig>(configData, JsonSettings.SerializerOptions);
                if (config != null)
                {
                    logger.LogInformation("Configuration loaded from: {ConfigPath}", configPath);
                    logger.LogInformation("Configuration contents: {@Config}", config);
                    return config;
                }
            }
        }


        logger.LogError("Error loading config.json. config.json must be in either: {Join}", string.Join(", ", PossibleConfigPaths));
        throw new Exception($"Error loading config.json. config.json must be in either: {string.Join(", ", PossibleConfigPaths)}");
    }

    public List<MediaFile> GetMediaFiles()
    {
        var files = new List<MediaFile>();

        try
        {
            if (!Directory.Exists(this.Config.FolderPath))
            {
                logger.LogError("Configured folder path does not exist: {FolderPath}", this.Config.FolderPath);
                return files;
            }

            var entries = Directory.GetFiles(this.Config.FolderPath);

            foreach (var filePath in entries)
            {
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.');

                if (AllSupportedExtensions.Contains(extension))
                {
                    files.Add(
                        new MediaFile
                        {
                            Name = fileName,
                            Path = filePath,
                            Type = SupportedVideoExtensions.Contains(extension) ? "video" : "image",
                        }
                    );
                }
            }

            // Sort files based on display order configuration
            files = this.Config.DisplayOrder?.ToLowerInvariant() switch
            {
                "random" => [.. files.OrderBy(_ => Random.Shared.Next())],
                "alpha" or _ => [.. files.OrderBy(static f => f.Name)]
            };
            
            logger.LogInformation("Found {FileCount} media files in {FolderPath}, sorted by {DisplayOrder}", 
                files.Count, this.Config.FolderPath, this.Config.DisplayOrder);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error reading media from folder: {FolderPath}", this.Config.FolderPath);
        }

        return files;
    }

    public Task<(Stream? stream, string contentType)> GetMediaFileStreamAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(this.Config.FolderPath, fileName);

            // Security check: ensure the file is within the configured folder
            var resolvedPath = Path.GetFullPath(filePath);
            var resolvedFolder = Path.GetFullPath(this.Config.FolderPath);

            if (!resolvedPath.StartsWith(resolvedFolder, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Access denied - path outside configured folder: {ResolvedPath}", resolvedPath);
                return Task.FromResult<(Stream? stream, string contentType)>((null, string.Empty));
            }

            if (!File.Exists(filePath))
            {
                logger.LogWarning("File not found: {FilePath}", filePath);
                return Task.FromResult<(Stream? stream, string contentType)>((null, string.Empty));
            }

            var fileInfo = new FileInfo(filePath);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                ".avi" => "video/avi",
                ".mov" => "video/quicktime",
                _ => "application/octet-stream",
            };

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            logger.LogDebug("Serving media file: {FileName}, Size: {FileSize} bytes", fileName, fileInfo.Length);

            return Task.FromResult<(Stream? stream, string contentType)>((stream, contentType));
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error serving media file: {FileName}", fileName);
            return Task.FromResult<(Stream? stream, string contentType)>((null, string.Empty));
        }
    }
}