using PebbleJar.Domain;
using PebbleJar.Application.Queries;

namespace PebbleJar.Application.Results;

public abstract record TransactionQueryResult(decimal TotalAmount);

public sealed record UngroupedTransactionQueryResult(
    PagedResult<Transaction> Transactions,
    decimal TotalAmount) : TransactionQueryResult(TotalAmount);

public sealed record GroupedTransactionQueryResult(
    PagedResult<GroupedTransactionResult> Groups,
    decimal TotalAmount) : TransactionQueryResult(TotalAmount);

public sealed record GroupedTransactionResult(
    GroupingKey Key,
    IReadOnlyList<Transaction> Transactions,
    decimal TotalAmount,
    int TotalCount);
