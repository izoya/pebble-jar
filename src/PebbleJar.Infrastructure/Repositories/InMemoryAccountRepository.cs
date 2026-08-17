using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using System.Collections.Concurrent;

namespace PebbleJar.Infrastructure.Repositories
{
    public class InMemoryAccountRepository() : IAccountRepository
    {
        private readonly ConcurrentDictionary<Guid, Account> _storage = [];

        public Task AddAsync(Account account)
        {
            if (!_storage.TryAdd(account.Id, account))
                throw new InvalidOperationException("An account with this ID already exists.");

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public Task<Account?> GetByIdAsync(Guid id)
        {
            _storage.TryGetValue(id, out Account? account);

            return Task.FromResult(account);
        }

        public Task<IReadOnlyList<Account>> ListAsync()
        {
            var list = _storage.Values.ToList().AsReadOnly();

            return Task.FromResult<IReadOnlyList<Account>>(list);
        }

        public Task UpdateAsync(Account account)
        {
            throw new NotImplementedException();
        }
    }
}
