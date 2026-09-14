using System.Globalization;
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
    private readonly DataScope Scope = DataScope.Transaction;

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

        await using var dbTransaction =
            await dbContext.Database.BeginTransactionAsync(token);

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
            return; // dbTransaction disposed
        }

        dbContext.Transactions.AddRange(missingTransactions);
        await dbContext.SaveChangesAsync(token);

        await versions.IncrementAsync(Scope);
        await dbTransaction.CommitAsync(token);
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

    public async Task<TransactionSearchQueryResult<Transaction>> ListAsync(
        TransactionQuery query,
        CancellationToken token)
    {
        await using var _dbTransactionHandler =
            await dbContext.Database.BeginSqliteDeferredTransactionAsync(token);

        var transactionsQuery = BuildFilteredQuery(query);
        var totalCount = await transactionsQuery.CountAsync(token);
        var totalAmount = await transactionsQuery.SumAsync(t => t.Amount, token);
        var revision = await versions.GetAsync(Scope);

        var items = await transactionsQuery
            .OrderByDescending(x => x.TransactionDateTime)
            .ThenByDescending(x => x.Id) // ensures consistent order
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(token);

        var pagination = new Pagination(
            query.PageNumber,
            query.PageSize,
            totalCount);

        return new TransactionSearchQueryResult<Transaction>(
            items,
            pagination,
            totalAmount,
            revision);
    }

    public async Task<TransactionSearchQueryResult<GroupedTransactionResult>> ListGroupsAsync(
        TransactionQuery query,
        TransactionGrouping grouping,
        CancellationToken token)
    {
        var isDateGrouping = grouping switch
        {
            TransactionGrouping.TransactionCategory or TransactionGrouping.TransactionType => false,
            TransactionGrouping.Day or TransactionGrouping.Week or TransactionGrouping.Fortnight
                or TransactionGrouping.Month or TransactionGrouping.Year => true,
            _ => throw new ArgumentOutOfRangeException(nameof(grouping)),
        };

        await using var transaction =
            await dbContext.Database.BeginSqliteDeferredTransactionAsync(token);

        var transactionsQuery = BuildFilteredQuery(query);
        var totalAmount = await transactionsQuery.SumAsync(t => t.Amount, token);

        var timestamped = transactionsQuery.Select(t => new TimestampedTransaction
        {
            // only select values used for grouping or aggregation
            Amount = t.Amount,
            Category = t.Category,
            Type = t.Type,
            LocalDate = null,
        });

        if (isDateGrouping)
        {
            // TransactionLocalDate view uses device timezone
            timestamped = transactionsQuery.Join(
                dbContext.Set<TransactionLocalDate>(),
                t => t.Id,
                d => d.TransactionId,
                (t, d) => new TimestampedTransaction
                {
                    Amount = t.Amount,
                    Category = t.Category,
                    Type = t.Type,
                    LocalDate = d.LocalDate,
                });
        }

        // Day numbers provide a SQL-translatable string key; restore the date after paging.
        var grouped = timestamped.GroupBy(grouping.Apply());

        var totalCount = await grouped.Select(g => g.Key).CountAsync(token);
        var ordered = grouping switch
        {
            TransactionGrouping.TransactionCategory =>
                grouped.OrderBy(g => g.Min(t => (int)t.Category)),
            TransactionGrouping.TransactionType =>
                grouped.OrderBy(g => g.Min(t => (int)t.Type)),
            // All the date groups
            _ => grouped.OrderByDescending(g => g.Min(t => t.LocalDate)),
        };
        var rows = await ordered
            .Select(g => new
            {
                g.Key,
                TotalCount = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
            })
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(token);

        var groups = rows.Select(row => new GroupedTransactionResult(
            grouping switch
            {
                TransactionGrouping.TransactionCategory => new GroupingKey(grouping,
                    (TransactionCategory)int.Parse(row.Key, CultureInfo.InvariantCulture)),

                TransactionGrouping.TransactionType => new GroupingKey(grouping,
                    (TransactionType)int.Parse(row.Key, CultureInfo.InvariantCulture)),

                _ => new GroupingKey(grouping, DateOnly.FromDayNumber(
                    int.Parse(row.Key, CultureInfo.InvariantCulture)).ToDateTime(TimeOnly.MinValue))
            }, row.TotalAmount, row.TotalCount)).ToList();

        var revision = await versions.GetAsync(Scope);

        return new TransactionSearchQueryResult<GroupedTransactionResult>(
            groups,
            new Pagination(query.PageNumber, query.PageSize, totalCount),
            totalAmount, revision);
    }

    private IQueryable<Transaction> BuildFilteredQuery(TransactionQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Transaction> transactionsQuery = dbContext.Transactions
            .AsNoTracking()
            .WhereIf(query.AccountId.HasValue, x => x.AccountId == query.AccountId)
            // this works because UTC normalization is configured for TransactionDateTime
            .WhereIf(query.FromDate.HasValue, x => x.TransactionDateTime >= query.FromDate)
            .WhereIf(query.ToDate.HasValue, x => x.TransactionDateTime <= query.ToDate)
            .WhereIf(query.AmountFrom.HasValue, x => x.Amount >= query.AmountFrom)
            .WhereIf(query.AmountTo.HasValue, x => x.Amount <= query.AmountTo)
            .WhereIf(query.TransactionType.HasValue, x => x.Type == query.TransactionType)
            .WhereIf(query.Categories is { } categories && categories.Length > 0,
                x => query.Categories!.Contains(x.Category))
            .WhereIf(query.TransactionKinds is { } kinds && kinds.Length > 0,
                x => query.TransactionKinds!.Contains(x.Kind));

        if (query.Query is { } queryStr)
        {
            var pattern = $"%{queryStr}%";

            transactionsQuery = transactionsQuery.Where(x =>
                EF.Functions.Like(x.Description, pattern) ||
                (x.RecognitionData != null &&
                    (EF.Functions.Like(x.RecognitionData.MerchantName, pattern) ||
                     EF.Functions.Like(x.RecognitionData.Reference, pattern) ||
                     EF.Functions.Like(x.RecognitionData.Particulars, pattern))));
        }

        return transactionsQuery;
    }
}
