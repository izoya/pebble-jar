using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Interfaces;
using PebbleJar.Application.Queries;
using PebbleJar.Application.Results;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Repositories;

public class SqliteTransactionRepository(PebbleJarDbContext dbContext) : ITransactionRepository
{
    public Task AddAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public async Task AddMissingAsync(
        Guid accountId,
        IEnumerable<Transaction> transactions,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        var transactionsToAdd = transactions.ToList();

        if (transactionsToAdd.Any(t => t.ExternalId is null))
        {
            throw new ArgumentException(
                "Unable to sync transactions list: at least one item is missing ExternalId."
                , nameof(transactions));
        }

        if (transactionsToAdd.Any(t => t.AccountId != accountId))
        {
            throw new ArgumentException(
                "All transactions must belong to the supplied account.",
                nameof(transactions));
        }

        var incomingExternalIds = transactionsToAdd
            .Select(t => t.ExternalId!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (incomingExternalIds.Length != transactionsToAdd.Count)
        {
            throw new ArgumentException(
                "Unable to sync transactions: the batch contains duplicate ExternalId values.",
                nameof(transactions));
        }

        var existingExternalIds = await dbContext.Transactions
            .AsNoTracking()
            .Where(t =>
                t.AccountId == accountId
                && t.ExternalId != null
                && incomingExternalIds.Contains(t.ExternalId))
            .Select(t => t.ExternalId)
            .ToListAsync(token);

        var missingTransactions = transactionsToAdd
            .Where(t => !existingExternalIds.Contains(t.ExternalId!))
            .ToList();

        if (missingTransactions.Count == 0)
        {
            return;
        }

        dbContext.Transactions.AddRange(missingTransactions);
        await dbContext.SaveChangesAsync(token);
    }

    public Task DeleteAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<DateTimeOffset?> GetLatestTransactionDateAsync(
        Guid accountId,
        CancellationToken token)
    {
        return dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .MaxAsync(t => (DateTimeOffset?)t.TransactionDateTime, token);
    }

    public async Task<PagedResult<Transaction>> ListAsync(
        TransactionQuery query,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Transaction> transactions = dbContext.Transactions
            .AsNoTracking();

        if (query.AccountId is { } accountId)
        {
            transactions = transactions.Where(t => t.AccountId == accountId);
        }

        if (query.FromDate is { } fromDate)
        {
            transactions = transactions.Where(
                t => t.TransactionDateTime >= fromDate);
        }

        if (query.ToDate is { } toDate)
        {
            transactions = transactions.Where(
                t => t.TransactionDateTime <= toDate);
        }

        var totalCount = await transactions.CountAsync(token);

        var items = await transactions
            .OrderByDescending(t => t.TransactionDateTime)
            .ThenByDescending(t => t.Id) // ens consistent pagination
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(token);

        return new PagedResult<Transaction>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount);
    }

    public Task UpdateAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }
}
