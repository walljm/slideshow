using System.Text.Json;

namespace SlideshowApp;

public class SlideshowService
{
    private readonly string[] _supportedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" };
    private readonly string[] _supportedVideoExtensions = { ".mp4", ".webm", ".ogg", ".avi", ".mov" };
    private readonly string[] _supportedExtensions;
    
    private SlideshowConfig _config = null!;
    private readonly string _configPath;

    public SlideshowService()
    {
        _supportedExtensions = _supportedImageExtensions.Concat(_supportedVideoExtensions).ToArray();
        _configPath = Path.Combine(FileSystem.AppDataDirectory, "config.json");
        _ = Task.Run(LoadConfigAsync); // Load config asynchronously without blocking
    }

    private async Task LoadConfigAsync()
    {
        try
        {
            // First try to load from the bundled WebAssets config.json
            var configJson = await LoadBundledConfig();
            if (!string.IsNullOrEmpty(configJson))
            {
                _config = JsonSerializer.Deserialize<SlideshowConfig>(configJson) ?? throw new InvalidOperationException("Failed to deserialize bundled config.");
            }
            else
            {
                // Fallback to app data directory config
                if (File.Exists(_configPath))
                {
                    var fileConfigJson = await File.ReadAllTextAsync(_configPath);
                    _config = JsonSerializer.Deserialize<SlideshowConfig>(fileConfigJson) ?? throw new InvalidOperationException("Failed to deserialize config from app data directory.");
                }
                else
                {
                    throw new FileNotFoundException("Configuration file not found in app data directory.", _configPath);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
        }
    }

    private async Task<string> LoadBundledConfig()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("WebAssets/config.json");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public SlideshowConfig GetConfig() => _config;

    public async Task<List<MediaFile>> GetMediaFilesAsync()
    {
        var files = new List<MediaFile>();

        try
        {
            if (!Directory.Exists(_config.FolderPath))
            {
                return files;
            }

            var directoryInfo = new DirectoryInfo(_config.FolderPath);
            var fileInfos = directoryInfo.GetFiles()
                .Where(f => _supportedExtensions.Contains(f.Extension.ToLowerInvariant()))
                .OrderBy(f => f.Name);

            foreach (var fileInfo in fileInfos)
            {
                var extension = fileInfo.Extension.ToLowerInvariant();
                var mediaType = _supportedVideoExtensions.Contains(extension) ? "video" : "image";
                
                // Convert file to base64 for embedding in HTML
                var base64Data = await ConvertFileToBase64Async(fileInfo.FullName);
                var mimeType = GetMimeType(extension);
                
                files.Add(new MediaFile
                {
                    Name = fileInfo.Name,
                    Path = $"data:{mimeType};base64,{base64Data}",
                    Type = mediaType
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading media files: {ex.Message}");
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
            _ => "application/octet-stream"
        };
    }
}

public class SlideshowConfig
{
    public int ImageDuration { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public int FadeTransitionDuration { get; set; }
    public bool AutoStart { get; set; }
    public bool ZoomOnImage { get; set; }
}

public class MediaFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
