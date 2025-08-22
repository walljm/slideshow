using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SlideshowWebServer;

internal sealed record MediaType(string ContentType, string Type);

public sealed class MediaService
{
    private static readonly Dictionary<string, MediaType> SupportedExtensions = new()
    {
        // image types
        { "png",  new MediaType("image/png",       "image") },
        { "jpg",  new MediaType("image/jpeg",      "image") },
        { "jpeg", new MediaType("image/jpeg",      "image") },
        { "gif",  new MediaType("image/gif",       "image") },
        { "svg",  new MediaType("image/svg+xml",   "image") },
        { "webp", new MediaType("image/webp",      "image") },
        { "bmp",  new MediaType("image/bmp",       "image") },
        // video types
        { "mp4",  new MediaType("video/mp4",       "video") },
        { "webm", new MediaType("video/webm",      "video") },
        { "ogg",  new MediaType("video/ogg",       "video") },
        { "avi",  new MediaType("video/avi",       "video") },
        { "mov",  new MediaType("video/quicktime", "video") },
    };

    private readonly ILogger<MediaService> logger;
    public SlideshowConfig Config { get;  }

    public MediaService(ILogger<MediaService> logger)
    {
        this.logger = logger;
        this.Config = LoadConfiguration();
    }

    private SlideshowConfig LoadConfiguration()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        if (File.Exists(configPath))
        {
            var configData = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<SlideshowConfig>(configData, JsonSettings.SerializerOptions);
            if (config != null)
            {
                logger.LogInformation("Configuration loaded from: {ConfigPath}", configPath);
                logger.LogInformation("Configuration contents: {@Config}", config);

                if (!Directory.Exists(config.FolderPath))
                {
                    logger.LogError("Configured folder path does not exist: {FolderPath}", this.Config.FolderPath);
                    throw new Exception($"Configured folder path does not exist: {this.Config.FolderPath}");
                }

                return config;
            }
        }

        // we want to fail catastrophically if the config file couldn't be found,
        //  since the config file is essential.
        logger.LogError("Error loading config.json. config.json must be in either: {ConfigPath}", configPath);
        throw new Exception($"Error loading config.json. config.json must be in either: {configPath}");
    }

    public List<MediaFile> GetMediaFiles()
    {
        var files = new List<MediaFile>();

        try
        {
            // this shouldn't happen, since we check on start,
            //  but it could happen if the user removes the folder after the service has started.
            // fail hard here.  we want the user to notice it has broken.
            if (!Directory.Exists(this.Config.FolderPath))
            {
                logger.LogError("Configured folder path does not exist: {FolderPath}", this.Config.FolderPath);
                throw new Exception($"Configured folder path does not exist: {this.Config.FolderPath}");
            }

            var entries = Directory.GetFiles(this.Config.FolderPath);

            foreach (var filePath in entries)
            {
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.');

                if (SupportedExtensions.TryGetValue(extension, out var supportedExtension))
                {
                    files.Add(
                        new MediaFile
                        {
                            Name = fileName,
                            Path = filePath,
                            Type = supportedExtension.Type,
                        }
                    );
                }
            }

            // Sort files based on display order configuration
            files = this.Config.DisplayOrder.ToLowerInvariant() switch
            {
                SlideshowConfig.RandomDisplayOrder => [.. files.OrderBy(static _ => Random.Shared.Next())],
                _ => [.. files.OrderBy(static f => f.Name)], // alpha is the default
            };

            logger.LogInformation(
                "Found {FileCount} media files in {FolderPath}, sorted by {DisplayOrder}",
                files.Count,
                this.Config.FolderPath,
                this.Config.DisplayOrder
            );
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error reading media from folder: {FolderPath}", this.Config.FolderPath);
        }

        return files;
    }

    private static readonly (Stream? stream, string contentType) ErrorMediaFileResult = (null, string.Empty);
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
                return Task.FromResult(ErrorMediaFileResult);
            }

            if (!File.Exists(filePath))
            {
                logger.LogWarning("File not found: {FilePath}", filePath);
                return Task.FromResult(ErrorMediaFileResult);
            }

            var fileInfo = new FileInfo(filePath);
            var extension = fileInfo.Extension.ToLowerInvariant().Trim('.');

            if (!SupportedExtensions.TryGetValue(extension, out var supportedExtension))
            {
                logger.LogWarning("File type not supported: {Extension}", extension);
                return Task.FromResult(ErrorMediaFileResult);
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            logger.LogDebug("Serving media file: {FileName}, Size: {FileSize} bytes", fileName, fileInfo.Length);

            return Task.FromResult<(Stream? stream, string contentType)>((stream, supportedExtension.ContentType));
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error serving media file: {FileName}", fileName);
            return Task.FromResult(ErrorMediaFileResult);
        }
    }
}