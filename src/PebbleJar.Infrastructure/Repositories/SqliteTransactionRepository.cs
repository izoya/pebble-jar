using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Interfaces;
using PebbleJar.Application.Queries;
using PebbleJar.Application.Results;
using PebbleJar.Domain;
using PebbleJar.Extensions;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Repositories;

public class SqliteTransactionRepository(
    PebbleJarDbContext dbContext,
    IDataVersionStore versions) : ITransactionRepository
{
    private DataScope Scope = DataScope.Transaction;

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

        if (incomingExternalIds.Length == 0)
        {
            return;
        }

        await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(token);

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
            return; // dbTransaction disposed; transaction rolled back
        }

        dbContext.Transactions.AddRange(missingTransactions);
        await dbContext.SaveChangesAsync(token);

        await versions.IncrementAsync(Scope);
        await dbTransaction.CommitAsync(token);
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

    public async Task<TransactionQueryResult> ListAsync(
        TransactionQuery query,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Transaction> qb = dbContext.Transactions
            .AsNoTracking();

        qb = qb.WhereIf(query.AccountId.HasValue, t => t.AccountId == query.AccountId)
            .WhereIf(query.FromDate.HasValue, t => t.TransactionDateTime >= query.FromDate)
            .WhereIf(query.ToDate.HasValue, t => t.TransactionDateTime <= query.ToDate)
            .WhereIf(query.AmountFrom.HasValue, t => t.Amount >= query.AmountFrom)
            .WhereIf(query.AmountTo.HasValue, t => t.Amount <= query.AmountTo)
            .WhereIf(query.TransactionType.HasValue, t => t.Type == query.TransactionType)
            .WhereIf(query.CategoryIds is { } catIds && catIds.Length > 0,
                t => query.CategoryIds.Contains(t.Category))
            .WhereIf(query.TransactionKindIds is { } kindIds && kindIds.Length > 0,
                t => query.TransactionKindIds.Contains(t.Kind));

        if (query.Query is { } queryStr)
        {
            var pattern = $"%{queryStr}%";

            qb = qb.Where(t =>
                EF.Functions.Like(t.Description, pattern) ||
                (t.RecognitionData != null &&
                    (EF.Functions.Like(t.RecognitionData.MerchantName, pattern) ||
                     EF.Functions.Like(t.RecognitionData.Reference, pattern))));
        }

        var totalCount = await qb.CountAsync(token);
        var totalAmount = await qb.SumAsync(t => t.Amount, token);

        var items = await qb
            .OrderByDescending(t => t.TransactionDateTime)
            .ThenByDescending(t => t.Id) // ens consistent pagination
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(token);

        var page = new PagedResult<Transaction>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount);

        return new TransactionQueryResult(page, totalAmount);
    }

    public Task UpdateAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }
}
