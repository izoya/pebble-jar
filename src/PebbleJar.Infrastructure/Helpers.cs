using PebbleJar.Domain;

namespace PebbleJar.Infrastructure;

public class Helpers
{
    public static (Account savingsAccount, Account testAccount) CreateAccounts()
    {
        var savingsAccount = new Account
        {
            Name = "Savings",
            AccountNumber = "12345",
            Currency = Currency.NZD,
        };
        var testAccount = new Account()
        {
            Name = "Test",
            AccountNumber = "fff",
            Currency = Currency.NZD,
        };

        return (savingsAccount, testAccount);
    }

}
