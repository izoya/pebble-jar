using PebbleJar.Application.Queries;
using PebbleJar.Domain;

namespace PebbleJar.Application.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken token);
    Task<IReadOnlyList<Account>> ListAsync(AccountQuery query, CancellationToken token);
    Task AddAsync(Account account);
    Task AddManyAsync(IEnumerable<Account> accounts);
    Task UpdateAsync(Account account);
    Task UpdateManyAsync(IEnumerable<Account> accounts);
    Task DeleteAsync(Account account);
}
