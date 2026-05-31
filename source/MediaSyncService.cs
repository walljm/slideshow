using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SlideshowWebServer;

/// <summary>
/// Mirrors files from <see cref="SlideshowConfig.FolderPath"/> (typically a remote
/// SMB share) into <see cref="SlideshowConfig.CacheFolderPath"/> on a timer.
///
/// Design goals:
///  - The HTTP layer never touches the source share at request time. All reads
///    are served from the local cache, so a slow or offline share cannot freeze
///    the slideshow.
///  - If the source is unavailable, we leave the cache alone and keep serving
///    the last known-good set. We retry on the next interval.
///  - File copies are atomic (write to a temp file, then rename) so a partially
///    copied file is never visible to readers.
///  - Per-file failures are isolated. One stuck/locked file does not abort the
///    whole sync pass.
/// </summary>
internal sealed class MediaSyncService : BackgroundService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // images
        "png", "jpg", "jpeg", "gif", "svg", "webp", "bmp",
        // videos
        "mp4", "webm", "ogg", "avi", "mov",
    };

    private readonly ILogger<MediaSyncService> logger;
    private readonly MediaService mediaService;

    public MediaSyncService(ILogger<MediaSyncService> logger, MediaService mediaService)
    {
        this.logger = logger;
        this.mediaService = mediaService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = this.mediaService.Config;
        var interval = TimeSpan.FromSeconds(Math.Max(5, config.SyncIntervalSeconds));

        logger.LogInformation(
            "MediaSyncService starting. Source={Source} Cache={Cache} Interval={Interval}s",
            config.FolderPath, config.CacheFolderPath, interval.TotalSeconds);

        // Make sure the cache exists before the first pass.
        try
        {
            Directory.CreateDirectory(config.CacheFolderPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not create cache folder {CacheFolderPath}", config.CacheFolderPath);
        }

        // Run an initial pass right away so a cold start populates the cache.
        await SyncOnce(config, stoppingToken);

        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SyncOnce(config, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
    }

    private async Task SyncOnce(SlideshowConfig config, CancellationToken cancellationToken)
    {
        var source = config.FolderPath;
        var cache = config.CacheFolderPath;

        // Probe the source. If it's gone, skip this pass (do NOT touch the cache).
        string[] sourceFiles;
        try
        {
            if (!Directory.Exists(source))
            {
                logger.LogWarning("Source folder unavailable, skipping sync: {Source}", source);
                return;
            }
            sourceFiles = Directory.GetFiles(source);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Source folder probe failed, skipping sync: {Source}", source);
            return;
        }

        // Build the set of supported source files and an index of cache file infos.
        var sourceByName = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in sourceFiles)
        {
            var ext = Path.GetExtension(path).TrimStart('.');
            if (!SupportedExtensions.Contains(ext)) continue;
            sourceByName[Path.GetFileName(path)] = new FileInfo(path);
        }

        var cacheByName = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.GetFiles(cache))
        {
            // ignore our temp files (atomic-write staging)
            if (Path.GetFileName(path).EndsWith(".tmp", StringComparison.Ordinal)) continue;
            cacheByName[Path.GetFileName(path)] = new FileInfo(path);
        }

        // Copy new / changed files.
        var copied = 0;
        var skipped = 0;
        var failed = 0;
        foreach (var (name, srcInfo) in sourceByName)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (cacheByName.TryGetValue(name, out var cachedInfo)
                && cachedInfo.Length == srcInfo.Length
                && cachedInfo.LastWriteTimeUtc == srcInfo.LastWriteTimeUtc)
            {
                skipped++;
                continue;
            }

            try
            {
                await CopyAtomic(srcInfo, Path.Combine(cache, name), cancellationToken);
                copied++;
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogWarning(ex, "Failed to copy {Name} from source to cache", name);
            }
        }

        // Remove cache files no longer present at source.
        var removed = 0;
        foreach (var name in cacheByName.Keys)
        {
            if (sourceByName.ContainsKey(name)) continue;
            try
            {
                File.Delete(Path.Combine(cache, name));
                removed++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to remove stale cache file {Name}", name);
            }
        }

        // Clean up any orphaned temp files from a prior failed copy.
        foreach (var path in Directory.GetFiles(cache, "*.tmp"))
        {
            try { File.Delete(path); } catch { /* best effort */ }
        }

        logger.LogInformation(
            "Sync complete: copied={Copied} skipped={Skipped} removed={Removed} failed={Failed} sourceCount={SourceCount}",
            copied, skipped, removed, failed, sourceByName.Count);
    }

    private static async Task CopyAtomic(FileInfo source, string destinationPath, CancellationToken cancellationToken)
    {
        var tempPath = destinationPath + ".tmp";

        // Open the source with FileShare.Read so we still race-lose cleanly if
        // the producer (e.g. NAS uploader) has it open for write \u2014 we just throw
        // and the outer loop logs and moves on.
        await using (var src = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true))
        await using (var dst = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
        {
            await src.CopyToAsync(dst, cancellationToken);
        }

        // Preserve mtime so our "changed?" check works.
        try
        {
            File.SetLastWriteTimeUtc(tempPath, source.LastWriteTimeUtc);
        }
        catch
        {
            // non-fatal; will just trigger a re-copy next pass
        }

        // Atomic replace on POSIX; on Windows File.Move with overwrite is also atomic enough for our purposes.
        File.Move(tempPath, destinationPath, overwrite: true);
    }
}
