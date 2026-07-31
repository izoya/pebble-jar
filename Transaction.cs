namespace ACount.Models;

public enum TransactionType
{
    Debit = 1,
    Credit = 2,
}

public class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    // TODO: This is an entry-creation date. Will need a transaction datetime as well
    public DateTime Date { get; init; } = DateTime.Now;
    // XXX: What is the difference between Debit and Credit? 
    public TransactionType Type { get; init; } = TransactionType.Debit;
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
