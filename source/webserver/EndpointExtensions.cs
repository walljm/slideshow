using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SlideshowWebServer;

public static class EndpointExtensions
{
    public static void AddSlideshowEndpoints(this IEndpointRouteBuilder app)
    {
        // API endpoints
        var api = app.MapGroup("/api");
        api.MapGet(
            "config",
            static (MediaService mediaService) =>
                Results.Json(mediaService.Config, JsonContext.Default.SlideshowConfig)
        );
        api.MapGet(
            "files",
            static (MediaService mediaService) =>
                Results.Json(mediaService.GetMediaFiles(), JsonContext.Default.ListMediaFile)
        );

        // Media file serving
        app.MapGet(
            "/media/{fileName}",
            static async (string fileName, MediaService mediaService) =>
            {
                var (stream, contentType) = await mediaService.GetMediaFileStreamAsync(fileName);
                return stream == null
                    ? Results.NotFound("File not found")
                    : Results.Stream(stream, contentType, fileName, DateTimeOffset.UtcNow, enableRangeProcessing: true);
            }
        );

        // Static file serving from embedded resources
        app.MapGet("/{*path}", DefaultHandler);
    }

    private static async Task<IResult> DefaultHandler(
        [FromServices] EmbeddedFileProvider fileProvider,
        [FromRoute] string? path
    )
    {
        // Default to index.html
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            path = "index.html";
        }

        try
        {
            var (content, contentType) = await fileProvider.GetFileAsync(path);
            return Results.Bytes(content, contentType);
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound($"File '{path}' not found");
        }
    }
}