using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Repositories;

public class SqliteDataVersionStore(PebbleJarDbContext dbContext) : IDataVersionStore
{
    public Task<int> GetAsync(DataScope scope)
    {
        return dbContext.DataVersions
            .Where(v => v.Scope == scope)
            .Select(v => v.Revision)
            .SingleOrDefaultAsync();
    }

    public async Task IncrementAsync(DataScope scope)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO data_versions (scope, revision)
            VALUES ({(int)scope}, 1)
            ON CONFLICT (scope)
            DO UPDATE SET revision = revision + 1;
            """);
    }
}
