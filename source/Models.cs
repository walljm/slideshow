namespace SlideshowWebServer;

public sealed class SlideshowConfig
{
    public int ImageDuration { get; set; } = 5;

    /// <summary>
    /// Source folder (e.g. an SMB mount). May be temporarily unavailable.
    /// Media is mirrored from here into <see cref="CacheFolderPath"/>.
    /// </summary>
    public string FolderPath { get; set; } = "./media";

    /// <summary>
    /// Local folder we serve from. Mirrored from <see cref="FolderPath"/>
    /// by the sync service. Must be on local storage and writable by the service.
    /// </summary>
    public string CacheFolderPath { get; set; } = "./cache";

    /// <summary>
    /// How often the sync service polls the source folder, in seconds.
    /// </summary>
    public int SyncIntervalSeconds { get; set; } = 60;

    public int FadeTransitionDuration { get; set; } = 1;
    public bool ZoomOnImage { get; set; } = true;
    public string DisplayOrder { get; set; } = AlphabeticalDisplayOrder;

    public const string AlphabeticalDisplayOrder = "alpha";
    public const string RandomDisplayOrder = "random";
}

public sealed class MediaFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}