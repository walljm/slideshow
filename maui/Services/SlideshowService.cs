using System.Diagnostics;
using System.Text.Json;

namespace SlideshowApp;

public sealed class SlideshowService
{
    private readonly string[] supportedImageExtensions = [ ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" ];
    private readonly string[] supportedVideoExtensions = [ ".mp4", ".webm", ".ogg", ".avi", ".mov" ];
    private readonly string[] supportedExtensions;

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // Try multiple locations for config.json
    private static readonly string[] possiblePaths =
    [
        Path.Combine(Environment.CurrentDirectory, "config.json"),
        Path.Combine(Environment.CurrentDirectory, "..", "config.json"),
    ];

    public SlideshowService()
    {
        this.Config = LoadConfig();
        supportedExtensions = supportedImageExtensions.Concat(supportedVideoExtensions).ToArray();
    }

    private SlideshowConfig LoadConfig()
    {
        foreach (var configPath in possiblePaths)
        {
            if (File.Exists(configPath))
            {
                var fileConfigJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<SlideshowConfig>(fileConfigJson, this.jsonSerializerOptions);
                if (config != null)
                {
                    return config;
                }
            }
        }

        throw new InvalidOperationException("No config file found");
    }

    public SlideshowConfig Config { get; }

    public SlideshowConfig GetConfig()
    {
        return this.Config;
    }

    public async Task<List<MediaFile>> GetMediaFilesAsync()
    {
        var files = new List<MediaFile>();

        try
        {
            // Ensure config is loaded
            if (string.IsNullOrEmpty(this.Config.FolderPath))
            {
                Debug.WriteLine("Config not loaded or folder path empty");
                return files;
            }

            if (!Directory.Exists(this.Config.FolderPath))
            {
                Debug.WriteLine($"Directory does not exist: {this.Config.FolderPath}");
                return files;
            }

            var directoryInfo = new DirectoryInfo(this.Config.FolderPath);
            var fileInfos = directoryInfo.GetFiles()
               .Where(f => supportedExtensions.Contains(f.Extension.ToLowerInvariant()))
               .OrderBy(static f => f.Name);

            foreach (var fileInfo in fileInfos)
            {
                var extension = fileInfo.Extension.ToLowerInvariant();
                var mediaType = supportedVideoExtensions.Contains(extension) ? "video" : "image";

                // Convert file to base64 for embedding in HTML
                var base64Data = await ConvertFileToBase64Async(fileInfo.FullName);
                var mimeType = GetMimeType(extension);

                files.Add(
                    new MediaFile
                    {
                        Name = fileInfo.Name,
                        Path = $"data:{mimeType};base64,{base64Data}",
                        Type = mediaType,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading media files: {ex.Message}");
        }

        return files;
    }

    private async Task<string> ConvertFileToBase64Async(string filePath)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "video/ogg",
            ".avi" => "video/avi",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream",
        };
    }
}

public sealed class SlideshowConfig
{
    public int ImageDuration { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public int FadeTransitionDuration { get; set; }
    public bool AutoStart { get; set; }
    public bool ZoomOnImage { get; set; }
}

public sealed class MediaFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}