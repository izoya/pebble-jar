using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;

namespace PebbleJar.Infrastructure.Repositories
{
    public class SqliteTransactionRepository : ITransactionRepository
    {
        public Task AddAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Transaction>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
