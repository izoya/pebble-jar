using PebbleJar.Application.Queries;
using PebbleJar.Application.Results;
using PebbleJar.Domain;

namespace PebbleJar.Application.Interfaces;

public interface ITransactionRepository
{
    Task<TransactionSearchQueryResult<Transaction>>
        ListAsync(
        TransactionQuery query,
        CancellationToken token);
    Task<TransactionSearchQueryResult<GroupedTransactionResult>>
        ListGroupsAsync(
        TransactionQuery query,
        TransactionGrouping grouping,
        CancellationToken token);
    Task
        AddMissingAsync(
        Guid accountId,
        IEnumerable<Transaction> transactions,
        CancellationToken token);
    Task<DateTimeOffset?>
        GetLatestTransactionDateAsync(
    Guid accountId,
    CancellationToken token);
}
