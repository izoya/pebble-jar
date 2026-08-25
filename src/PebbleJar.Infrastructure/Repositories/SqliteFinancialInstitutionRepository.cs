using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;

namespace PebbleJar.Infrastructure.Repositories
{
    internal class SqliteFinancialInstitutionRepository : IFinancialInstitutionRepository
    {
        public Task AddAsync(FinancialInstitution institution)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(FinancialInstitution institution)
        {
            throw new NotImplementedException();
        }

        public Task<FinancialInstitution?> GetByIdAsync(Guid id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<FinancialInstitution>> ListAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(FinancialInstitution institution)
        {
            throw new NotImplementedException();
        }
    }
}
