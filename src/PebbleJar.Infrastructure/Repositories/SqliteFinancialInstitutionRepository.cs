using Microsoft.EntityFrameworkCore;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Repositories
{
    public sealed class SqliteFinancialInstitutionRepository(PebbleJarDbContext dbContext) : IFinancialInstitutionRepository
    {
        public async Task AddAsync(FinancialInstitution institution)
        {
            ArgumentNullException.ThrowIfNull(institution);

            dbContext.FinancialInstitutions.Add(institution);
            await dbContext.SaveChangesAsync();
        }

        public Task DeleteAsync(FinancialInstitution institution)
        {
            throw new NotImplementedException();
        }

        public async Task AddManyAsync(
            IEnumerable<FinancialInstitution> institutions)
        {
            ArgumentNullException.ThrowIfNull(institutions);

            dbContext.FinancialInstitutions.AddRange(institutions);
            await dbContext.SaveChangesAsync();
        }

        public Task<FinancialInstitution?> GetByIdAsync(Guid id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<FinancialInstitution>> ListAsync(CancellationToken token)
        {
            return await dbContext.FinancialInstitutions
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(token);
        }

        public Task UpdateAsync(FinancialInstitution institution)
        {
            throw new NotImplementedException();
        }
    }
}
