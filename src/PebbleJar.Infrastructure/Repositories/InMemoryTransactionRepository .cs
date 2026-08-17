using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using System.Collections.Concurrent;

namespace PebbleJar.Infrastructure.Repositories
{
    public class InMemoryTransactionRepository() : ITransactionRepository
    {
        private readonly ConcurrentDictionary<Guid, Transaction> _storage = [];

        public Task<Transaction?> GetByIdAsync(Guid id)
        {
            _storage.TryGetValue(id, out var transaction);

            return Task.FromResult(transaction);
        }

        public Task<IReadOnlyList<Transaction>> ListAsync()
        {
            return Task.FromResult<IReadOnlyList<Transaction>>(_storage.Values.ToList().AsReadOnly());

        }

        public Task AddAsync(Transaction transaction)
        {
            if (!_storage.TryAdd(transaction.Id, transaction))
                throw new InvalidOperationException("A transaction with this ID already exists.");

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
