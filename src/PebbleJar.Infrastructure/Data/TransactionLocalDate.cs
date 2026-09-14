namespace PebbleJar.Infrastructure.Data;

internal sealed class TransactionLocalDate
{
    public Guid TransactionId { get; init; }
    public DateOnly LocalDate { get; init; }
}
