using PebbleJar.Application.Queries;

namespace PebbleJar.Application.Results;

public sealed record GroupedTransactionResult(
    GroupingKey Key,
    decimal TotalAmount,
    int TotalCount);

public sealed record TransactionSearchQueryResult<TItem>(
    IReadOnlyList<TItem> Items,
    Pagination Pagination,
    decimal TotalAmount,
    int DataVersion);
