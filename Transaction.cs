namespace ACount.Models;

class Transaction
{
    public Guid Id { get; } = Guid.NewGuid();
    public required Guid AccountId { get; set; }

    public DateTime Date { get; } = DateTime.Now;
    public string? Description { get; set; }

    public required decimal Amount { get; set; }

    public override string ToString()
    {
        return $"[Transaction] AccountId: {AccountId}, Amount: {Amount,9} {"on " + Date,30}";
    }
}
