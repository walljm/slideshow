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

    public List<string> GetMediaFileNames()
    {
        var fileNames = new List<string>();

        try
        {
            // Ensure config is loaded
            if (string.IsNullOrEmpty(this.Config.FolderPath))
            {
                Debug.WriteLine("Config not loaded or folder path empty");
                return fileNames;
            }

            if (!Directory.Exists(this.Config.FolderPath))
            {
                Debug.WriteLine($"Directory does not exist: {this.Config.FolderPath}");
                return fileNames;
            }

            var directoryInfo = new DirectoryInfo(this.Config.FolderPath);
            var fileInfos = directoryInfo.GetFiles()
               .Where(f => supportedExtensions.Contains(f.Extension.ToLowerInvariant()))
               .OrderBy(static f => f.Name);

            fileNames.AddRange(fileInfos.Select(f => f.Name));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading media file names: {ex.Message}");
        }

        return fileNames;
    }

    public async Task<MediaFile?> GetMediaFileAsync(string fileName)
    {
        try
        {
            // Ensure config is loaded
            if (string.IsNullOrEmpty(this.Config.FolderPath))
            {
                Debug.WriteLine("Config not loaded or folder path empty");
                return null;
            }

            if (!Directory.Exists(this.Config.FolderPath))
            {
                Debug.WriteLine($"Directory does not exist: {this.Config.FolderPath}");
                return null;
            }

            var filePath = Path.Combine(this.Config.FolderPath, fileName);
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"File does not exist: {filePath}");
                return null;
            }

            // Security check: ensure the file is within the configured folder
            var resolvedPath = Path.GetFullPath(filePath);
            var resolvedFolder = Path.GetFullPath(this.Config.FolderPath);
            
            if (!resolvedPath.StartsWith(resolvedFolder, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Access denied - file outside configured folder: {fileName}");
                return null;
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!supportedExtensions.Contains(extension))
            {
                Debug.WriteLine($"Unsupported file extension: {extension}");
                return null;
            }

            var mediaType = supportedVideoExtensions.Contains(extension) ? "video" : "image";

            // Convert file to base64 for embedding in HTML
            var base64Data = await ConvertFileToBase64Async(filePath);
            var mimeType = GetMimeType(extension);

            return new MediaFile
            {
                Name = fileName,
                Path = $"data:{mimeType};base64,{base64Data}",
                Type = mediaType,
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading media file {fileName}: {ex.Message}");
            return null;
        }
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