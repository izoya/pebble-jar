using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Interfaces;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Repositories
{
    public class SqliteAccountRepository(
        PebbleJarDbContext dbContext)
        : IAccountRepository
    {
        public async Task AddAsync(Account account)
        {
            ArgumentNullException.ThrowIfNull(account);

            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddManyAsync(IEnumerable<Account> accounts)
        {
            ArgumentNullException.ThrowIfNull(accounts);

            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateManyAsync(IEnumerable<Account> accounts)
        {
            ArgumentNullException.ThrowIfNull(accounts);

            dbContext.Accounts.UpdateRange(accounts);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public async Task<Account?> GetByIdAsync(Guid id, CancellationToken token)
        {
            return await dbContext.Accounts
                .SingleOrDefaultAsync(x => x.Id == id, token);
        }

        public async Task<IReadOnlyList<Account>> ListAsync(
            AccountQuery query,
            CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(query);

            IQueryable<Account> accounts = dbContext.Accounts.AsNoTracking();

            if (query.Ids is { } ids && ids.Length > 0)
                accounts = accounts.Where(acc => ids.Contains(acc.Id));


            if (query.IsSyncEnabled is { } sync)
                accounts = accounts.Where(acc => acc.IsSyncEnabled == sync);

            if (query.WithProvider is { } withProvider && withProvider == true)
                // Eager loading
                accounts = accounts.Include(acc => acc.FinancialInstitution);

            return await accounts
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Name)
                .ToListAsync(token);
        }

        public async Task UpdateAsync(Account account)
        {
            ArgumentNullException.ThrowIfNull(account);

            dbContext.Accounts.Update(account);
            await dbContext.SaveChangesAsync();
        }
    }
}
