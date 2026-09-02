using PebbleJar.Domain.Abstractions;
using System.Text.Json;

namespace PebbleJar.Domain;

public enum Currency
{
    NZD = 1,
    USD = 2,
}
public enum AccountStatus
{
    Active = 1,
    Inactive = 2,
}

public class Account : IAuditable
{
    /// <summary>
    /// Inner ID
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>
    /// Account ID from connection provider.
    /// </summary>
    public string? ExternalId { get; init; }

    public required string Name
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ?
            throw new ArgumentException("Account Name could not be empty")
            : value;
    }
    public string? AccountNumber { get; init; }

    public required Guid FinancialInstitutionId { get; init; }
    public FinancialInstitution FinancialInstitution { get; init; } = null!; // 1:M

    public ConnectionProvider? ConnectionProvider { get; init; }



    public required AccountStatus Status { get; set; }

    public bool IsSyncEnabled { get; private set; }

    public Currency Currency { get; set; } = Currency.NZD;

    public string? SourcePayloadJson { get; private set; }

    public DateTimeOffset? SourceFetchedAt { get; private set; }

    public ICollection<Transaction> Transactions { get; init; } = [];

    public void SetSyncEnabled(bool isSyncEnabled) => IsSyncEnabled = isSyncEnabled;

    //private decimal CurrentBalance;

    public override string ToString()
    {
        return $"[Account] Name: {Name}, AccountNumber: {AccountNumber}, Currency: {Currency}..";
    }

    public void SetPayloadJson<T>(T payload)
    {
        this.SourcePayloadJson = JsonSerializer.Serialize(payload);
        this.SourceFetchedAt = DateTime.UtcNow;
    }
}
