using PebbleJar.Domain;

namespace PebbleJar.Application.Results;

public sealed record TransactionQueryResult(
    PagedResult<Transaction> Transactions,
    decimal TotalAmount);
