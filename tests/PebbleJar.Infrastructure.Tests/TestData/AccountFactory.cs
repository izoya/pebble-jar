using PebbleJar.Domain;
using PebbleJar.Infrastructure.Data;

namespace PebbleJar.Infrastructure.Tests.TestData;

internal static class AccountFactory
{
    public static async Task<Account> CreateAsync(PebbleJarDbContext context)
    {
        var institution = new FinancialInstitution { Name = "Test bank" };
        var account = new Account
        {
            Name = "Test account",
            FinancialInstitutionId = institution.Id,
            Status = AccountStatus.Active,
        };

        context.AddRange(institution, account);
        await context.SaveChangesAsync(CancellationToken.None);

        return account;
    }
}
