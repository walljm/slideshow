using System.Text.Json;
using System.Text.Json.Serialization;
using SlideshowWebServer.Models;

namespace SlideshowWebServer;

[JsonSerializable(typeof(SlideshowConfig))]
[JsonSerializable(typeof(List<MediaFile>))]
public partial class JsonContext : JsonSerializerContext
{
}

public static class JsonSettings
{
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
      AllowTrailingCommas = true,
      PropertyNameCaseInsensitive = false,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      TypeInfoResolver = JsonContext.Default,
    };
}