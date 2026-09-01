namespace PebbleJar.Domain;

// Reference List
public class FinancialInstitution
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ?
            throw new ArgumentException("FinancialInstitution Name could not be empty")
            : value;
    }

    public ConnectionProvider? ConnectionProvider { get; init; }

    public string? ExternalId { get; init; }

    public ICollection<Account> Accounts { get; init; } = [];
}
