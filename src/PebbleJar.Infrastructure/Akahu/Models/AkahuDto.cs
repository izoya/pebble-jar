using System.Text.Json;
using System.Text.Json.Serialization;

namespace PebbleJar.Infrastructure.Akahu.Models;

public abstract class AkahuDto
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> UnknownFields { get; init; } = [];
}
