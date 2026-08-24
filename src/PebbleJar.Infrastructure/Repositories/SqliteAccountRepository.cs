using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;

namespace PebbleJar.Infrastructure.Repositories
{
    public class SqliteAccountRepository : IAccountRepository
    {
        public Task AddAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public Task<Account?> GetByIdAsync(Guid id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Account>> ListAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Account account)
        {
            throw new NotImplementedException();
        }
    }
}
