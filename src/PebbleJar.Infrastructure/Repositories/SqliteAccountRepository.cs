using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Interfaces;
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
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken token)
        {
            return await dbContext.Accounts
                // No changes tracking required
                //.AsNoTracking()
                // Eager loading
                .Include(account => account.FinancialInstitution)
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Name)
                .ToListAsync(token);
        }

        public async Task UpdateAsync(Account account)
        {
            throw new NotImplementedException();
        }
    }
}
