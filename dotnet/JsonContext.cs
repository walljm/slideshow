using System.Text.Json.Serialization;
using SlideshowWebServer.Models;

namespace SlideshowWebServer;

[JsonSerializable(typeof(SlideshowConfig))]
[JsonSerializable(typeof(List<MediaFile>))]
public partial class JsonContext : JsonSerializerContext
{
}