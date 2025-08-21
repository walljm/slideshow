namespace SlideshowWebServer.Models;

public sealed class SlideshowConfig
{
    public int ImageDuration { get; set; } = 5;
    public string FolderPath { get; set; } = "./media";
    public int FadeTransitionDuration { get; set; } = 1;
    public bool AutoStart { get; set; } = true;
    public bool ZoomOnImage { get; set; } = true;
}

public sealed class MediaFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}