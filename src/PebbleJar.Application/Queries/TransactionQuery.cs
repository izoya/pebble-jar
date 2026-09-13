using PebbleJar.Domain;

namespace PebbleJar.Application.Queries;

public sealed record TransactionQuery
{
    public Guid? AccountId { get; }
    public DateTimeOffset? FromDate { get; }
    public DateTimeOffset? ToDate { get; }
    public string? Query { get; }
    public decimal? AmountFrom { get; }
    public decimal? AmountTo { get; }

    public TransactionType? TransactionType { get; }
    public TransactionCategory[]? CategoryIds { get; }
    public TransactionKind[]? TransactionKindIds { get; }

    public int PageNumber { get; }
    public int PageSize { get; }

    public TransactionQuery(
        Guid? accountId,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        decimal? amountFrom = null,
        decimal? amountTo = null,
        string? query = null,
        TransactionType? transactionType = null,
        TransactionCategory[]? categoryIds = null,
        TransactionKind[]? transactionKindIds = null,
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

        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            throw new ArgumentException(
                "FromDate must be earlier than or equal toDate ToDate.",
                nameof(fromDate));
        }


        if (amountFrom.HasValue && amountTo.HasValue && amountFrom > amountTo)
        {
            throw new ArgumentException(
                "AmountFrom must be less than or equal AmountTo.",
                 nameof(amountFrom));
        }

        AccountId = accountId;
        FromDate = fromDate;
        ToDate = toDate;
        AmountFrom = amountFrom;
        AmountTo = amountTo;
        Query = query;
        TransactionType = transactionType;
        CategoryIds = categoryIds;
        TransactionKindIds = transactionKindIds;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
