using System.Text.Json.Serialization;

namespace PebbleJar.Infrastructure.Akahu.Models;

public sealed class AkahuUser : AkahuDto
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }

    [JsonPropertyName("access_granted_at")]
    public required DateTimeOffset AccessGrantedAt { get; init; }
}
