using PebbleJar.Domain;

namespace PebbleJar.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id, CancellationToken token);
        Task<IReadOnlyList<Account>> ListAsync(CancellationToken token);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(Account account);
    }
}
