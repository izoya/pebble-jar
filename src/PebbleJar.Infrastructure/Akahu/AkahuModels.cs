using System.Text.Json;
using System.Text.Json.Serialization;

namespace PebbleJar.Infrastructure.Akahu.Models;

public abstract class AkahuDto
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> UnknownFields { get; init; } = [];
}

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

    public override IEnumerable<AkahuDto> ListItems() => Items;
}

public sealed class AkahuUser : AkahuDto
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }

    [JsonPropertyName("access_granted_at")]
    public required DateTimeOffset AccessGrantedAt { get; init; }
}

public sealed class AkahuAccount : AkahuDto
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }
    /// <summary>
    /// Only present for accounts migrated from legacy
    /// to open-banking connection.
    /// </summary>
    [JsonPropertyName("_migrated")]
    public string? MigratedFromId { get; init; }

    /// <summary>
    /// Authorisation with a financial institute.
    /// </summary>
    /// <remarks>
    /// Ex.: accounts from one bank would most likely 
    /// have the same AuthorisationId.
    /// 
    /// Can also be used to revoke access to connected accounts.
    /// </remarks>
    [JsonPropertyName("_authorisation")]
    public string? AuthorisationId { get; init; }

    /// <summary>
    /// <b>[Deprecated]</b> Use `AuthorisationId` instead.
    /// </summary>
    [JsonPropertyName("_credentials")]
    public string? CredentialsId { get; init; }

    /// <summary>
    /// Account provider (financial institute) info.
    /// </summary>
    [JsonPropertyName("connection")]
    public AkahuConnection? Connection { get; init; }

    /// <summary>
    /// Account name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Formatted account number if defined. 
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Absent for KiwiSaver accounts.</item>
    /// <item><c>00-0000-0000000-00</c> for NZ banks.</item>
    /// <item>Redacted <c>****-****-****-1234</c> for credit cards.</item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("formatted_account")]
    public string? FormattedAccount { get; init; }

    [JsonPropertyName("status")]
    public AkahuAccountStatus? Status { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("attributes")]
    public IReadOnlyList<string> Attributes { get; init; } = [];

    [JsonPropertyName("balance")]
    public AkahuBalance? Balance { get; init; }

    [JsonPropertyName("refreshed")]
    public AkahuRefreshed? Refreshed { get; init; }

    [JsonPropertyName("meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("payment_consents")]
    public JsonElement? PaymentConsents { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<AkahuAccountStatus>))]
public enum AkahuAccountStatus
{
    [JsonStringEnumMemberName("ACTIVE")]
    Active,
    [JsonStringEnumMemberName("INACTIVE")]
    Inactive,
}

/// <summary>
/// Holds information about original account provider.
/// </summary>
public sealed class AkahuConnection
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("logo")]
    public Uri? Logo { get; init; }

    [JsonPropertyName("connection_type")]
    public string? ConnectionType { get; init; }
}

public sealed class AkahuBalance
{
    [JsonPropertyName("currency")]
    public required string Currency { get; init; }

    [JsonPropertyName("current")]
    public decimal? Current { get; init; }

    [JsonPropertyName("available")]
    public decimal? Available { get; init; }

    [JsonPropertyName("limit")]
    public decimal? Limit { get; init; }

    [JsonPropertyName("overdrawn")]
    public bool? Overdrawn { get; init; }
}

public sealed class AkahuRefreshed
{
    [JsonPropertyName("balance")]
    public DateTimeOffset? Balance { get; init; }
    [JsonPropertyName("meta")]
    public DateTimeOffset? Meta { get; init; }
    [JsonPropertyName("transactions")]
    public DateTimeOffset? Transactions { get; init; }
    [JsonPropertyName("party")]
    public DateTimeOffset? Party { get; init; }
}
