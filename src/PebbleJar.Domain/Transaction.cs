using PebbleJar.Domain.Abstractions;
using System.Text.Json;

namespace PebbleJar.Domain;

public enum TransactionType
{
    Debit = 1,
    Credit = 2,
}

public class Transaction : IAuditable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>
    /// Account ID from connection provider
    /// </summary>
    public string? ExternalId { get; init; }
    /// <summary>
    /// Inner Account Id
    /// </summary>
    public required Guid AccountId { get; init; }
    /// <summary>
    /// Account ID from connection provider
    /// </summary>
    public string? ExternalAccountId { get; set; }
    /// <summary>
    /// The timestamp of when this transaction was created by the bank
    /// </summary>
    public required DateTimeOffset TransactionDateTime { get; init; }
    /// <summary>
    /// User's description
    /// </summary>
    public string? Description { get; set; }

    public required decimal Amount
    {
        get;
        init
        {
            field = value;
            Type = value < 0
                ? TransactionType.Debit
                : TransactionType.Credit;
        }
    }

    public TransactionType Type { get; private set; }

    public required TransactionCategory Category { get; set; } = TransactionCategory.Default;
    public required TransactionKind Kind { get; init; }

    public TransactionRecognitionData? RecognitionData { get; init; }

    public string? SourcePayloadJson { get; private set; }
    public DateTimeOffset? SourceFetchedAt { get; private set; }

    public virtual Account Account { get; init; } = null!;

    public void SetPayloadJson<T>(T payload)
    {
        this.SourcePayloadJson = JsonSerializer.Serialize(payload);
        this.SourceFetchedAt = DateTime.UtcNow;
    }
}

public enum TransactionKind
{
    Unspecified = 0,
    GenericDebit,
    GenericCredit,
    CardPayment,
    ExternalPayment,
    Transfer,
    StandingOrder,
    DirectDebit,
    DirectCredit,
    CashWithdrawalOrDeposit,
    Interest,
    Fee,
    Tax,
    CreditCardPayment,
    LoanPayment,
}

// TODO: Turn this into extendable entity
public enum TransactionCategory
{
    Default = 0,
    Food = 1,
    Household = 2,
    Housing = 3,
    Utilities = 4,
    Transport = 5,
    Health = 6,
    Fitness = 7,
    Education = 8,
    Lifestyle = 9,
    ProfessionalServices = 10,
    Income = 11,
    InterestIncome = 12,
    Taxes = 13,
    SavingsAndInvestments = 14,
    Miscellaneous = 15,
}

/// <summary>
/// Immutable collection of values from data provider's DTO.
/// Filled on a best-effort principle.
/// </summary>
public record TransactionRecognitionData
{
    public string? MerchantName { get; init; }
    public string? Category { get; init; }
    public string? Group { get; init; }
    public string? CardSuffix { get; init; }
    public string? Code { get; init; }
    public string? Reference { get; init; }
    public string? Particulars { get; init; }
    public string? CounterpartyAccount { get; init; }
}
