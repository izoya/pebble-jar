using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Tests.Fixtures;

/// <summary>
/// A fresh in-memory SQLite database for each test.
/// </summary>
internal sealed class InMemorySqliteDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private InMemorySqliteDatabase(
        SqliteConnection connection,
        PebbleJarDbContext context)
    {
        this.connection = connection;
        Context = context;
    }

    public PebbleJarDbContext Context { get; }

    public static async Task<InMemorySqliteDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);

        var options = new DbContextOptionsBuilder<PebbleJarDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;
        var context = new PebbleJarDbContext(options);
        await context.Database.EnsureCreatedAsync(CancellationToken.None);

        return new InMemorySqliteDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }
}
