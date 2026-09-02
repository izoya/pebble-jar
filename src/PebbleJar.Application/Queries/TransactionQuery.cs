namespace PebbleJar.Application.Queries;

public sealed record TransactionQuery
{
    public Guid? AccountId { get; }
    public DateTimeOffset? FromDate { get; }
    public DateTimeOffset? ToDate { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public TransactionQuery(
        Guid? accountId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Page number must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        if (from is { } fromDate && to is { } toDate && fromDate > toDate)
        {
            throw new ArgumentException(
                "FromDate must be earlier than or equal to ToDate.",
                nameof(from));
        }

        AccountId = accountId;
        FromDate = from;
        ToDate = to;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
