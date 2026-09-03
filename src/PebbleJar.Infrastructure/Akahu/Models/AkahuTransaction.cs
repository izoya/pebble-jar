using System.Text.Json.Serialization;

namespace PebbleJar.Infrastructure.Akahu.Models;

public sealed class AkahuTransaction : AkahuDto
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }
    [JsonPropertyName("_account")]
    public required string AccountId { get; init; }
    [JsonPropertyName("_user")]
    public required string UserId { get; init; }
    [JsonPropertyName("_connection")]
    /// <summary>
    /// Original account provider ID
    /// </summary>
    public required string ConnectionId { get; init; }

    #region Dates
    /// <summary>
    /// The ISO 8601 timestamp of when this transaction was retrieved and created by Akahu
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// The ISO 8601 timestamp of when this transaction was last updated by Akahu
    /// </summary>
    [JsonPropertyName("updated_at")]
    public required DateTimeOffset UpdatedAt { get; init; }
    /// <summary>
    /// The ISO 8601 timestamp of when this transaction was created by the bank
    /// </summary>
    [JsonPropertyName("date")]
    public required DateTimeOffset Date { get; init; }
    #endregion 


    [JsonPropertyName("description")]
    public required string Description { get; init; }
    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }
    /// <summary>
    /// The account balance after the receipt of this transaction, where available
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal? Balance { get; init; }
    [JsonPropertyName("type")]
    public required AkahuTransactionType Type { get; init; }
    /// <summary>
    /// The merchant that generated this transaction
    /// </summary>
    [JsonPropertyName("merchant")]
    public AkahuMerchant? Merchant { get; init; }
    /// <summary>
    /// The base NZFCC category that the transaction belongs to.
    /// </summary>
    [JsonPropertyName("category")]
    public AkahuTransactionCategory? Category { get; init; }
    /// <summary>
    /// Additional transaction metadata.
    /// </summary>
    /// <remarks>
    /// All fields are optional and provided on a "best-effort" basis.
    /// </remarks>
    [JsonPropertyName("meta")]
    public AkahuTransactionMeta? Meta { get; init; }


    [JsonPropertyName("posted_date")]
    public DateTimeOffset PostedDate { get; init; }


    /// <summary>
    /// The Akahu Transaction ID that this transaction was migrated from.
    /// </summary>
    [JsonPropertyName("_migrated")]
    public string? MigratedFromId { get; init; }
    /// <summary>
    /// The Akahu Account ID that this transaction was migrated from.
    /// </summary>
    [JsonPropertyName("_migrated_account")]
    public string? MigratedFromAccountId { get; init; }

    [Obsolete("This field is deprecated and should not be used")]
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<AkahuTransactionType>))]
public enum AkahuTransactionType
{
    /// <summary>
    /// Money has entered the account
    /// </summary>
    [JsonStringEnumMemberName("CREDIT")] Credit,
    /// <summary>
    /// Money has left the account
    /// </summary>
    [JsonStringEnumMemberName("DEBIT")] Debit,
    /// <summary>
    /// Payment to an external account
    /// </summary>
    [JsonStringEnumMemberName("PAYMENT")] Payment,
    /// <summary>
    /// Transfer between accounts that are associated with the same credentials
    /// </summary>
    [JsonStringEnumMemberName("TRANSFER")] Transfer,
    /// <summary>
    /// Automatic payment
    /// </summary>
    [JsonStringEnumMemberName("STANDING ORDER")] StandingOrder,
    /// <summary>
    /// Payment made via an EFTPOS system
    /// </summary>
    [JsonStringEnumMemberName("EFTPOS")] Eftpos,
    /// <summary>
    /// An interest payment
    /// </summary>
    [JsonStringEnumMemberName("INTEREST")] Interest,
    /// <summary>
    /// Fee payment to the account provider
    /// </summary>
    [JsonStringEnumMemberName("FEE")] Fee,
    /// <summary>
    /// Tax payment
    /// </summary>
    [JsonStringEnumMemberName("TAX")] Tax,
    /// <summary>
    /// Credit card payment
    /// </summary>
    [JsonStringEnumMemberName("CREDIT CARD")] CreditCard,
    /// <summary>
    /// Direct credit (someone paying into the account)
    /// </summary>
    [JsonStringEnumMemberName("DIRECT CREDIT")] DirectCredit,
    /// <summary>
    /// Direct debit payment
    /// </summary>
    [JsonStringEnumMemberName("DIRECT DEBIT")] DirectDebit,
    /// <summary>
    /// ATM deposit or withdrawal
    /// </summary>
    [JsonStringEnumMemberName("ATM")] Atm,
    /// <summary>
    /// Loan payment
    /// </summary>
    [JsonStringEnumMemberName("LOAN")] Loan,
}


public class AkahuTransactionMeta : AkahuDto
{
    /// <summary>
    /// The particulars field set on this transaction
    /// </summary>
    [JsonPropertyName("particulars")]
    public string? Particulars { get; init; }
    /// <summary>
    /// The code field set on this transaction
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }
    /// <summary>
    /// The reference field set on this transaction
    /// </summary>
    [JsonPropertyName("reference")]
    public string? Reference { get; init; }
    /// <summary>
    /// The formatted NZ bank account number of the other party to this transaction
    /// </summary>
    [JsonPropertyName("other_account")]
    public string? OtherAccount { get; init; }
    /// <summary>
    /// If this transaction was made with a credit or debit card, the last four digits of the card number
    /// </summary>
    [JsonPropertyName("card_suffix")]
    public string? CardSuffix { get; init; }
    /// <summary>
    /// URL of an image for this transaction (typically the merchant's logo).
    /// If no logo is available, a placeholder image is provided.
    /// </summary>
    [JsonPropertyName("logo")]
    public string? Logo { get; init; }
    /// <summary>
    /// If this transaction was made in another currency, details about the currency conversion
    /// </summary>
    [JsonPropertyName("conversion")]
    public AkahuConversion? Conversion { get; init; }
}

public class AkahuMerchant : AkahuDto
{
    [JsonPropertyName("_id")]
    public string? Id { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("website")]
    public string? Website { get; init; }
    [JsonPropertyName("nzbn")]
    public string? Nzbn { get; init; }
}

public class AkahuTransactionCategory : AkahuDto
{
    [JsonPropertyName("_id")]
    public string? Id { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    /// <summary>
    /// Higher level groupings that a category belongs to.
    /// </summary>
    /// <remarks>
    /// By default Akahu will include <c>personal_finance</c>.
    /// </remarks>
    [JsonPropertyName("groups")]
    public Dictionary<string, AkahuTransactionGroup>? Groups { get; init; }
}

public class AkahuTransactionGroup : AkahuDto
{
    [JsonPropertyName("_id")]
    public string? Id { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

public class AkahuConversion : AkahuDto
{
    /// <summary>
    /// The amount transacted in the foreign currency
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }
    /// <summary>
    /// The (3 letter ISO 4217 currency code)[https://www.xe.com/iso4217.php] 
    /// that was used for this transaction
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }
    /// <summary>
    /// The foreign currency conversion rate applied to this transaction
    /// </summary>
    [JsonPropertyName("rate")]
    public decimal? Rate { get; init; }
}
