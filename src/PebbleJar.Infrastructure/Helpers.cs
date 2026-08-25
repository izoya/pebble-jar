using PebbleJar.Domain;

namespace PebbleJar.Infrastructure;

public class Helpers
{
    public static FinancialInstitution CreateTestBank()
    {
        return new FinancialInstitution() { Id = Guid.NewGuid(), Name = "Test Bank" };
    }

    public static (Account savingsAccount, Account testAccount) CreateAccounts()
    {
        var bank = CreateTestBank();
        var savingsAccount = new Account
        {
            Name = "Savings",
            AccountNumber = "12345",
            Currency = Currency.NZD,
            FinancialInstitutionId = bank.Id,
            Status = AccountStatus.Active,
            
        };
        var testAccount = new Account()
        {
            Name = "Test",
            AccountNumber = "fff",
            Currency = Currency.NZD,
            FinancialInstitutionId = bank.Id,
            Status = AccountStatus.Active,
        };

        return (savingsAccount, testAccount);
    }

}
