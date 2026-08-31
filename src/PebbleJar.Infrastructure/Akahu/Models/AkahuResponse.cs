using System.Text.Json.Serialization;

namespace PebbleJar.Infrastructure.Akahu.Models;

public abstract class AkahuResponseBase : AkahuDto
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    public abstract IEnumerable<AkahuDto> ListItems();
}

public sealed class AkahuSingleResponse<T> : AkahuResponseBase
    where T : AkahuDto
{
    [JsonPropertyName("item")]
    public required T Item { get; init; }

    public override IEnumerable<AkahuDto> ListItems() => [Item];
}

public sealed class AkahuListResponse<T> : AkahuResponseBase
    where T : AkahuDto
{
    [JsonPropertyName("items")]
    public required IReadOnlyList<T> Items { get; init; }
    [JsonPropertyName("cursor")]
    public AkahuCursor? Cursor { get; init; }

    public override IEnumerable<AkahuDto> ListItems() => Items;
}

public sealed class AkahuCursor
{
    [JsonPropertyName("next")]
    public string? Next { get; init; }
}
