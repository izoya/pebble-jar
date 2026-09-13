using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace PebbleJar.Infrastructure.Data;

public static class SqliteReadTransactionExtensions
{
    /// <summary>
    /// Custom Sqlite Deferred transactions handler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The extension keeps all reads consistent without reserving the database writer slot.
    /// </para>
    /// <para>
    /// With <c>Microsoft.Data.Sqlite</c>, equivalent behavior could be achieved using
    /// <c>BeginTransactionAsync(IsolationLevel.ReadUncommitted, token) </c>
    /// only when the connection is configured with <c>Cache=Private</c>.
    /// </para>
    /// <list type="bullet">
    /// <item>For read operations only - does not exposes commit.</item>
    /// <item>Not suitable for concurrent operations on the same context.</item>
    /// </list>
    /// </remarks>
    public static async Task<IAsyncDisposable> BeginSqliteDeferredTransactionAsync(
        this DatabaseFacade database,
        CancellationToken token = default)
    {
        if (database.GetDbConnection() is not SqliteConnection connection)
            throw new InvalidOperationException("SQLite connection is required.");

        // Ensure there's no nested transactions in current DB context (request-scoped). 
        if (database.CurrentTransaction is not null)
            throw new InvalidOperationException("A transaction is already active.");

        await database.OpenConnectionAsync(token);

        SqliteTransaction? transaction = null;

        try
        {
            transaction = connection.BeginTransaction(deferred: true);

            // Pass created transaction to EF
            var efWrapper = await database.UseTransactionAsync(transaction, token)
                ?? throw new InvalidOperationException("Could not enlist the SQLite read transaction.");

            return new SqliteTransactionGuard(database, transaction, efWrapper);
        }
        catch
        {
            try
            {
                if (transaction is not null)
                    await transaction.DisposeAsync();
            }
            finally
            {
                await database.CloseConnectionAsync();
            }
            throw; // propagate the original failure
        }
    }

    /// <summary>
    /// Holds and disposes transaction resources.
    /// </summary>
    private sealed class SqliteTransactionGuard(
        DatabaseFacade database,
        SqliteTransaction transaction,
        IDbContextTransaction efWrapper) : IAsyncDisposable
    {
        private bool disposed;

        public async ValueTask DisposeAsync()
        {
            // Prevent repeated cleanup.
            // For example, with `await guard.DisposeAsync()` followed by scope exit.

            if (disposed) return;
            disposed = true;

            // Nested try-finally to ensure every dispose call attempted.
            try
            {
                await efWrapper.DisposeAsync();
            }
            finally
            {
                try
                {
                    // No commit: this scope is for reads only.
                    await transaction.DisposeAsync();
                }
                finally
                {
                    await database.CloseConnectionAsync();
                }
            }
        }
    }
}
