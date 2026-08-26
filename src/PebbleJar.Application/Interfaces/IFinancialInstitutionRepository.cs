using PebbleJar.Domain;

namespace PebbleJar.Application.Interfaces;

public interface IFinancialInstitutionRepository
{
    Task<FinancialInstitution?> GetByIdAsync(Guid id, CancellationToken token);
    Task<IReadOnlyList<FinancialInstitution>> ListAsync(CancellationToken token);
    Task AddAsync(FinancialInstitution institution);
    Task AddManyAsync(IEnumerable<FinancialInstitution> institutions);
    Task UpdateAsync(FinancialInstitution institution);
    Task DeleteAsync(FinancialInstitution institution);
}
