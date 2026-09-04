using Microsoft.EntityFrameworkCore;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Repositories;
using PebbleJar.Infrastructure.Tests.Fixtures;
using PebbleJar.Infrastructure.Tests.TestData;
using Xunit;

namespace PebbleJar.Infrastructure.Tests.Repositories;

public sealed class SqliteTransactionRepositoryTests
{
    [Fact]
    public async Task AddMissingAsync_RepeatedTransaction_StoresItOnce()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context);

        await repository.AddMissingAsync(
            account.Id,
            [CreateTransaction(account.Id, "transaction-1")],
            CancellationToken.None);
        await repository.AddMissingAsync(
            account.Id,
            [CreateTransaction(account.Id, "transaction-1")],
            CancellationToken.None);

        var stored = await database.Context.Transactions.ToListAsync();
        Assert.Single(stored);
        Assert.Equal("transaction-1", stored[0].ExternalId);
    }

    [Fact]
    public async Task AddMissingAsync_DuplicateExternalIdsInBatch_Throws()
    {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var account = await AccountFactory.CreateAsync(database.Context);
        var repository = new SqliteTransactionRepository(database.Context);
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
        var repository = new SqliteTransactionRepository(database.Context);

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
        var repository = new SqliteTransactionRepository(database.Context);

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

    private static Transaction CreateTransaction(Guid accountId, string? externalId) =>
        new()
        {
            AccountId = accountId,
            ExternalId = externalId,
            TransactionDateTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Description = "Test transaction",
            Amount = -10m,
            Category = TransactionCategory.Default,
            Kind = TransactionKind.GenericDebit,
        };

}
