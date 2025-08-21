using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;

namespace SlideshowWebServer;

public sealed class EmbeddedFileProvider
{
    private readonly Assembly assembly;
    private readonly string baseNamespace;

    public EmbeddedFileProvider()
    {
        assembly = Assembly.GetExecutingAssembly();
        baseNamespace = "SlideshowWebServer.wwwroot";
    }

    public async Task<(byte[] content, string contentType)> GetFileAsync(string path)
    {
        // Remove leading slash and convert to resource name
        var resourcePath = path.TrimStart('/').Replace('/', '.');
        var resourceName = $"{baseNamespace}.{resourcePath}";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
        }

        var content = new byte[stream.Length];
        await stream.ReadExactlyAsync(content, 0, content.Length);

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return (content, contentType);
    }
}