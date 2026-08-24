using System.Text.Json;
using System.Text.Json.Serialization;

namespace PebbleJar.Infrastructure.Akahu.Models;

public abstract class AkahuDto
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> UnknownFields { get; init; } = [];
}

public sealed class AkahuResponse<T> : AkahuDto
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("item")]
    public required T Item { get; init; }
}

public sealed class AkahuUser : AkahuDto
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }

    [JsonPropertyName("access_granted_at")]
    public required DateTimeOffset AccessGrantedAt { get; init; }
}
