using Microsoft.Extensions.Logging;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using System.Collections.Concurrent;

namespace PebbleJar.Infrastructure.Repositories
{
    public class InMemoryAccountRepository(ILogger<InMemoryAccountRepository> logger) : IAccountRepository
    {
        private readonly ConcurrentDictionary<Guid, Account> _storage = [];
        private static Task ImitateDelayedResponse(CancellationToken token)
        {
            return Task.Delay(TimeSpan.FromSeconds(15), token);
        }

        public Task AddAsync(Account account)
        {
            if (!_storage.TryAdd(account.Id, account))
                throw new InvalidOperationException("An account with this ID already exists.");

            logger.LogInformation("Account added: {AccountId}", account.Id);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public async Task<Account?> GetByIdAsync(Guid id, CancellationToken token)
        {
            _storage.TryGetValue(id, out Account? account);
            await ImitateDelayedResponse(token);

            return account;
        }

        public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken token)
        {
            var list = _storage.Values.ToList().AsReadOnly();
            await ImitateDelayedResponse(token);

            return list;
        }

        public Task UpdateAsync(Account account)
        {
            throw new NotImplementedException();
        }
    }
}
