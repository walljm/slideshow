using Microsoft.AspNetCore.Http.HttpResults;
using SlideshowWebServer.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlideshowWebServer;

[JsonSerializable(typeof(SlideshowConfig))]
[JsonSerializable(typeof(List<MediaFile>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(FileStreamHttpResult))]
[JsonSerializable(typeof(FileContentHttpResult))]
[JsonSerializable(typeof(NotFound<string>))]
public partial class JsonContext : JsonSerializerContext
{
}

public static class JsonSettings
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
      AllowTrailingCommas = true,
      PropertyNameCaseInsensitive = false,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      TypeInfoResolver = JsonContext.Default,
    };
}