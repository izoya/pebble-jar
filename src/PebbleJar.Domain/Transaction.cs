namespace PebbleJar.Domain;


public enum TransactionType
{
    Debit = 1,
    Credit = 2,
}

// TODO: Turn this into extendable entity
public enum TransactionCategory
{
    Default = 0,
    Groceries = 1,
    Utilities = 2,
    Entertainment = 3,
    Transportation = 4,
    Healthcare = 5,
    Education = 6,
    Miscellaneous = 7,
}

public class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    // TODO: This is an entry-creation date. Will need a transaction datetime as well
    public DateTime Date { get; init; } = DateTime.Now;
    public TransactionCategory Category { get; init; } = TransactionCategory.Default;

    public TransactionType Type => Amount < 0 ? TransactionType.Debit : TransactionType.Credit;

    public string? Description
    {
        get;
        init => field = string.IsNullOrWhiteSpace(value) ?
            throw new ArgumentException("Transaction description could not be empty string")
            : value;
    }

    public override string ToString()
    {
        return $"[Transaction] AccountId: {AccountId}, Amount: {Amount,9} {"on " + Date,30}";
    }
}
