using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SlideshowWebServer;

public static class EndpointExtensions
{
    public static void AddSlideshowEndpoints(this IEndpointRouteBuilder app)
    {
        // API endpoints
        var api = app.MapGroup("/api");
        api.MapGet("config", static (MediaService mediaService) => Results.Json(mediaService.Config));
        api.MapGet("files", static (MediaService mediaService) => Results.Json(mediaService.GetMediaFiles()));

        // Media file serving
        app.MapGet("/media/{fileName}", GetMediaFiles);

        // Static file serving from embedded resources
        app.MapGet("/{*path}", DefaultHandler);
    }

    private static async Task<Results<FileStreamHttpResult, NotFound<string>>> GetMediaFiles(
        [FromServices] MediaService mediaService,
        [FromRoute] string fileName
    )
    {
        var (stream, contentType) = await mediaService.GetMediaFileStreamAsync(fileName);
        return stream is null
            ? TypedResults.NotFound("File not found")
            : TypedResults.Stream(stream, contentType, fileName, DateTimeOffset.UtcNow, enableRangeProcessing: true);
    }

    private static async Task<Results<FileContentHttpResult, NotFound<string>>> DefaultHandler(
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
            return TypedResults.Bytes(content, contentType);
        }
        catch (FileNotFoundException)
        {
            return TypedResults.NotFound($"File '{path}' not found");
        }
    }
}