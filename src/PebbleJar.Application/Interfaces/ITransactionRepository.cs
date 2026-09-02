using PebbleJar.Application.Queries;
using PebbleJar.Application.Results;
using PebbleJar.Domain;

namespace PebbleJar.Application.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<PagedResult<Transaction>> ListAsync(
        TransactionQuery query,
        CancellationToken token);
    Task AddAsync(Transaction transaction);
    Task AddMissingAsync(
        Guid accountId,
        IEnumerable<Transaction> transactions,
        CancellationToken token);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);

    Task<DateTimeOffset?> GetLatestTransactionDateAsync(
    Guid accountId,
    CancellationToken token);
}
