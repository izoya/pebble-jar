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

    private sealed record TimestampedTransaction(
        Transaction Transaction,
        DateTimeOffset LocalTimestamp);

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

        IQueryable<Transaction> qb = dbContext.Transactions.AsNoTracking();

        qb = qb.WhereIf(query.AccountId.HasValue, x => x.AccountId == query.AccountId)
            .WhereIf(query.FromDate.HasValue, x => x.TransactionDateTime >= query.FromDate)
            .WhereIf(query.ToDate.HasValue, x => x.TransactionDateTime <= query.ToDate)
            .WhereIf(query.AmountFrom.HasValue, x => x.Amount >= query.AmountFrom)
            .WhereIf(query.AmountTo.HasValue, x => x.Amount <= query.AmountTo)
            .WhereIf(query.TransactionType.HasValue, x => x.Type == query.TransactionType)
            .WhereIf(query.CategoryIds is { } catIds && catIds.Length > 0,
                x => query.CategoryIds!.Contains(x.Category))
            .WhereIf(query.TransactionKindIds is { } kindIds && kindIds.Length > 0,
                x => query.TransactionKindIds!.Contains(x.Kind));

        if (query.Query is { } queryStr)
        {
            var pattern = $"%{queryStr}%";
            qb = qb.Where(x =>
                EF.Functions.Like(x.Description, pattern) ||
                (x.RecognitionData != null &&
                    (EF.Functions.Like(x.RecognitionData.MerchantName, pattern) ||
                     EF.Functions.Like(x.RecognitionData.Reference, pattern))));
        }

        if (query.Grouping is { } grouping)
        {
            var timestampedTransactions = (await qb.ToListAsync(token))
                .Select(transaction => new TimestampedTransaction(
                    transaction,
                    TimeZoneInfo.ConvertTime(
                        transaction.TransactionDateTime.ToUniversalTime(),
                        query.TimeZone)));

            var groupedQb = timestampedTransactions
                .GroupBy(x => grouping.Apply(x.Transaction, x.LocalTimestamp));

            return IntoGroupedTransactionResults(query, groupedQb);
        }

        var totalCount = await qb.CountAsync(token);
        var totalAmount = await qb.SumAsync(t => t.Amount, token);

        var items = await qb
            .OrderByDescending(x => x.TransactionDateTime)
            .ThenByDescending(x => x.Id) // ensures consistent pagination
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(token);

        var page = new PagedResult<Transaction>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount);

        return new UngroupedTransactionQueryResult(page, totalAmount);
    }

    private static GroupedTransactionQueryResult IntoGroupedTransactionResults(
        TransactionQuery query,
        IEnumerable<IGrouping<GroupingKey, TimestampedTransaction>> groupedQb)
    {
        var groups = OrderGroups(groupedQb)
            .Select(group =>
            {
                var transactions = group
                    .OrderByDescending(x => x.LocalTimestamp)
                    .ThenByDescending(x => x.Transaction.Id)
                    .Select(x => x.Transaction)
                    .ToList();

                return new GroupedTransactionResult(
                    group.Key,
                    transactions,
                    transactions.Sum(x => x.Amount),
                    transactions.Count);
            })
            .ToList();

        var page = new PagedResult<GroupedTransactionResult>(
            groups
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList(),
            query.PageNumber,
            query.PageSize,
            groups.Count);

        return new GroupedTransactionQueryResult(
            page,
            groups.Sum(x => x.TotalAmount));
    }

    private static IOrderedEnumerable<IGrouping<GroupingKey, TimestampedTransaction>> OrderGroups(
        IEnumerable<IGrouping<GroupingKey, TimestampedTransaction>> groups)
    {
        return groups.OrderByDescending(group => group.Key switch
        {
            GroupingKey.Date key => key.Value,
            _ => DateTime.MinValue,
        }).ThenBy(group => group.Key switch
        {
            GroupingKey.Type key => (int)key.Value,
            GroupingKey.Category key => (int)key.Value,
            _ => 0,
        });
    }

    public Task UpdateAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }
}
