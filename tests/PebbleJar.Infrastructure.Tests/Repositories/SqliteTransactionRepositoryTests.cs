using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Repositories;
using PebbleJar.Infrastructure.Tests.Fixtures;
using PebbleJar.Infrastructure.Tests.TestData;
using Xunit;

namespace PebbleJar.Infrastructure.Tests.Repositories;

public sealed class SqliteTransactionRepositoryTests
{
    [Fact]
    public async Task SearchGroups_KeysAreIndependentOfCurrentCulture()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, new SqliteDataVersionStore(database.Context));
        var transaction = CreateTransaction(account.Id, "culture");
        transaction.Category = TransactionCategory.ProfessionalServices;
        await repository.AddMissingAsync(account.Id, [transaction], CancellationToken.None);
        var query = new TransactionQuery(account.Id);
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            foreach (var grouping in Enum.GetValues<TransactionGrouping>())
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                var expected = await repository.ListGroupsAsync(query, grouping, CancellationToken.None);

                var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                culture.NumberFormat.PositiveSign = "1";
                culture.NumberFormat.NegativeSign = "7";
                CultureInfo.CurrentCulture = culture;
                var actual = await repository.ListGroupsAsync(query, grouping, CancellationToken.None);

                Assert.Equal(Assert.Single(expected.Items).Key, Assert.Single(actual.Items).Key);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task Search_PagesGroupsAndItemsIndependently_WithFilteredTotals()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var versions = new SqliteDataVersionStore(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, versions);
        var first = CreateTransaction(account.Id, "a");
        first.Category = TransactionCategory.Household;
        var second = CreateTransaction(account.Id, "b");
        second.Category = TransactionCategory.Household;
        var third = CreateTransaction(account.Id, "c");
        third.Category = TransactionCategory.ProfessionalServices;
        await repository.AddMissingAsync(account.Id, [first, second, third], CancellationToken.None);
        await database.Context.Database.ExecuteSqlRawAsync("DROP VIEW transaction_local_dates");

        var query = new TransactionQuery(account.Id, pageSize: 1);
        var groups = await repository.ListGroupsAsync(query, TransactionGrouping.TransactionCategory, CancellationToken.None);
        Assert.Equal(2, groups.Pagination.TotalCount);
        var household = Assert.Single(groups.Items);
        Assert.Equal(TransactionCategory.Household, household.Key.Value.AsT2);
        Assert.Equal(2, household.TotalCount);
        Assert.Equal(-20m, household.TotalAmount);
        Assert.Equal(-30m, groups.TotalAmount);
        Assert.Equal(1, groups.DataVersion);

        var page = await repository.ListAsync(new TransactionQuery(account.Id,
            categories: [TransactionCategory.Household], pageSize: 1), CancellationToken.None);
        Assert.Single(page.Items);
        Assert.Equal(2, page.Pagination.TotalCount);
        Assert.Equal(-20m, page.TotalAmount);
        Assert.Equal(groups.DataVersion, page.DataVersion);

        var secondGroupPage = await repository.ListGroupsAsync(
            new TransactionQuery(account.Id, pageNumber: 2, pageSize: 1),
            TransactionGrouping.TransactionCategory, CancellationToken.None);
        Assert.Equal(TransactionCategory.ProfessionalServices, Assert.Single(secondGroupPage.Items).Key.Value.AsT2);

        var empty = await repository.ListGroupsAsync(new TransactionQuery(account.Id, query: "no match"),
            TransactionGrouping.TransactionCategory, CancellationToken.None);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.Pagination.TotalCount);
        Assert.Equal(0m, empty.TotalAmount);
    }

    [Fact]
    public async Task SearchGroups_DayBoundaryUsesDeviceTimeZone()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, new SqliteDataVersionStore(database.Context));
        var localMidnight = new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Unspecified);
        var utcMidnight = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localMidnight));
        var entries = new[] { -1, 1 }.Select(minutes => CreateTransaction(
            account.Id, $"midnight-{minutes}", amount: -5m,
            timestamp: utcMidnight.AddMinutes(minutes))).ToList();

        await repository.AddMissingAsync(account.Id, entries, CancellationToken.None);
        var result = await repository.ListGroupsAsync(
            new TransactionQuery(account.Id),
            TransactionGrouping.Day, CancellationToken.None);
        Assert.Equal(2, result.Pagination.TotalCount);
        Assert.Equal(new[] { new DateTime(2026, 9, 28), new DateTime(2026, 9, 27) },
            result.Items.Select(group => group.Key.Value.AsT0));
        Assert.Equal(entries.Count, result.Items.Sum(group => group.TotalCount));
    }

    [Fact]
    public async Task SearchGroups_TypeGroupingOrdersNumericallyWithoutDateView()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, new SqliteDataVersionStore(database.Context));
        var debit = CreateTransaction(account.Id, "debit");
        var credit = CreateTransaction(account.Id, "credit", amount: 20m);
        await repository.AddMissingAsync(account.Id, [credit, debit], CancellationToken.None);
        await database.Context.Database.ExecuteSqlRawAsync("DROP VIEW transaction_local_dates");

        var result = await repository.ListGroupsAsync(new TransactionQuery(account.Id),
            TransactionGrouping.TransactionType, CancellationToken.None);

        Assert.Equal(2, result.Pagination.TotalCount);
        Assert.Equal(10m, result.TotalAmount);
        Assert.Equal(-10m, result.Items[0].TotalAmount);
        Assert.Equal(20m, result.Items[1].TotalAmount);
        Assert.Equal(TransactionType.Debit, result.Items[0].Key.Value.AsT1);
        Assert.Equal(TransactionType.Credit, result.Items[1].Key.Value.AsT1);
    }

    [Fact]
    public async Task SearchGroups_MonthGroupingPagesChronologicallyWithFilteredTotals()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var otherAccount = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, new SqliteDataVersionStore(database.Context));
        var entries = new[] { 1, 2, 3 }.Select(index => CreateTransaction(
            account.Id, $"date-{index}", amount: -index,
            timestamp: new DateTimeOffset(2023 + index, 6, 15, 12, 0, 0, TimeSpan.Zero))).ToList();
        var excluded = CreateTransaction(account.Id, "excluded", amount: -100m);
        await repository.AddMissingAsync(account.Id, [.. entries, excluded], CancellationToken.None);
        await repository.AddMissingAsync(otherAccount.Id,
            [CreateTransaction(otherAccount.Id, "other", amount: -2m)], CancellationToken.None);

        var result = await repository.ListGroupsAsync(
            new TransactionQuery(account.Id, amountFrom: -3m, amountTo: -1m, pageNumber: 2, pageSize: 1),
            TransactionGrouping.Month, CancellationToken.None);

        Assert.Equal(3, result.Pagination.TotalCount);
        Assert.Equal(-6m, result.TotalAmount);
        var group = Assert.Single(result.Items);
        Assert.Equal(new DateTime(2025, 6, 1), group.Key.Value.AsT0);
        Assert.Equal(-2m, group.TotalAmount);
        Assert.Equal(1, group.TotalCount);

        var empty = await repository.ListGroupsAsync(new TransactionQuery(account.Id, query: "no match"),
            TransactionGrouping.Month, CancellationToken.None);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.Pagination.TotalCount);
        Assert.Equal(0m, empty.TotalAmount);
    }

    [Fact]
    public async Task SearchGroups_FortnightHandlesDateBeforeAnchor()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, new SqliteDataVersionStore(database.Context));
        var localTimestamp = new DateTime(1970, 1, 4, 12, 0, 0, DateTimeKind.Unspecified);
        var timestamp = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localTimestamp));
        var transaction = CreateTransaction(account.Id, "anchor", timestamp: timestamp);
        await repository.AddMissingAsync(account.Id, [transaction], CancellationToken.None);

        var result = await repository.ListGroupsAsync(new TransactionQuery(account.Id),
            TransactionGrouping.Fortnight, CancellationToken.None);

        Assert.Equal(new DateTime(1969, 12, 22), Assert.Single(result.Items).Key.Value.AsT0);
    }

    [Theory]
    [InlineData(TransactionGrouping.Week, 2025, 12, 29)]
    [InlineData(TransactionGrouping.Fortnight, 2025, 12, 22)]
    [InlineData(TransactionGrouping.Year, 2026, 1, 1)]
    public async Task SearchGroups_CalendarBucketStartsAreCorrect(
        TransactionGrouping grouping, int year, int month, int day)
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, new SqliteDataVersionStore(database.Context));
        var localTimestamp = new DateTime(2026, 1, 4, 12, 0, 0, DateTimeKind.Unspecified);
        var transaction = CreateTransaction(account.Id, "a",
            timestamp: new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localTimestamp)));
        await repository.AddMissingAsync(account.Id, [transaction], CancellationToken.None);
        var result = await repository.ListGroupsAsync(
            new TransactionQuery(account.Id),
            grouping, CancellationToken.None);
        Assert.Equal(new DateTime(year, month, day), Assert.Single(result.Items).Key.Value.AsT0);
    }

    [Fact]
    public async Task AddMissingAsync_NormalizesTransactionTimestampToUtc()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(
            database.Context,
            new SqliteDataVersionStore(database.Context));
        var transaction = new Transaction
        {
            AccountId = account.Id,
            ExternalId = "non-utc",
            TransactionDateTime = new DateTimeOffset(
                2026, 1, 2, 1, 0, 0, TimeSpan.FromHours(13)),
            Amount = -10m,
            Category = TransactionCategory.Default,
            Kind = TransactionKind.GenericDebit,
        };

        await repository.AddMissingAsync(account.Id, [transaction], CancellationToken.None);

        Assert.Equal(TimeSpan.Zero, transaction.TransactionDateTime.Offset);

        await using var command = database.Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT transaction_date_time FROM transactions WHERE external_id = 'non-utc'";
        var storedTimestamp = Assert.IsType<string>(await command.ExecuteScalarAsync());
        Assert.EndsWith("+00:00", storedTimestamp);
    }

    [Fact]
    public async Task AddMissingAsync_RepeatedTransaction_StoresItOnce()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var versions = new SqliteDataVersionStore(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, versions);

        await repository.AddMissingAsync(
            account.Id,
            [CreateTransaction(account.Id, "transaction-1")],
            CancellationToken.None);
        Assert.Equal(1, await versions.GetAsync(DataScope.Transaction));

        await repository.AddMissingAsync(
            account.Id,
            [CreateTransaction(account.Id, "transaction-1")],
            CancellationToken.None);

        var stored = await database.Context.Transactions.ToListAsync();
        Assert.Single(stored);
        Assert.Equal("transaction-1", stored[0].ExternalId);
        Assert.Equal(1, await versions.GetAsync(DataScope.Transaction));
    }

    [Fact]
    public async Task AddMissingAsync_DuplicateExternalIdsInBatch_Throws()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var versions = new SqliteDataVersionStore(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, versions);
        var transactions = new[]
        {
            CreateTransaction(account.Id, "transaction-1"),
            CreateTransaction(account.Id, "transaction-1"),
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.AddMissingAsync(
                account.Id,
                transactions,
                CancellationToken.None));

        Assert.Contains("duplicate ExternalId", exception.Message);
    }

    [Fact]
    public async Task AddMissingAsync_TransactionForAnotherAccount_Throws()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var versions = new SqliteDataVersionStore(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, versions);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.AddMissingAsync(
                account.Id,
                [CreateTransaction(Guid.NewGuid(), "transaction-1")],
                CancellationToken.None));

        Assert.Contains("All transactions must belong", exception.Message);
    }

    [Fact]
    public async Task AddMissingAsync_TransactionWithoutExternalId_Throws()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var versions = new SqliteDataVersionStore(database.Context);
        var repository = new SqliteTransactionRepository(database.Context, versions);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.AddMissingAsync(
                account.Id,
                [CreateTransaction(account.Id, externalId: null)],
                CancellationToken.None));

        Assert.Contains("missing ExternalId", exception.Message);
    }

    [Fact]
    public async Task Database_UniqueAccountAndExternalIdIndex_RejectsDuplicates()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);

        database.Context.Transactions.Add(
            CreateTransaction(account.Id, "transaction-1"));
        await database.Context.SaveChangesAsync(CancellationToken.None);

        database.Context.Transactions.Add(
            CreateTransaction(account.Id, "transaction-1"));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            database.Context.SaveChangesAsync(CancellationToken.None));
    }

    private static Transaction CreateTransaction(
        Guid accountId,
        string? externalId,
        decimal amount = -10m,
        DateTimeOffset? timestamp = null) =>
        new()
        {
            AccountId = accountId,
            ExternalId = externalId,
            TransactionDateTime = timestamp ?? new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Description = "Test transaction",
            Amount = amount,
            Category = TransactionCategory.Default,
            Kind = TransactionKind.GenericDebit,
        };
}
