using ACount.Models;

namespace ACount;

internal class Program
{
    static void Main()
    {
        var account = new Account
        {
            Name = "Savings",
            AccountNumber = "12345",
            Currency = Currency.NZD,
        };
        Console.WriteLine($"{account}\n");

        var transactions = new List<Transaction> {
            new Transaction { AccountId = account.Id, Amount = -100m },
            new Transaction { AccountId = account.Id, Amount = -50.25m, Description = "Groceries" },
            new Transaction { AccountId = account.Id, Amount = 20m, Description = "Refund" },
        };

        foreach (var transaction in transactions)
        {
            Console.WriteLine($"{transaction}");
        }

        var acc = new Account() { AccountNumber = "fff", Name = "Test", Currency = Currency.NZD };
        var commonId = Guid.NewGuid();
        var commonDate = DateTime.Now;
        Transaction tr = new() { AccountId = acc.Id, Amount = 45.0m, Description = "Something", Id = commonId, Date = commonDate };
        Transaction tr2 = new() { AccountId = acc.Id, Amount = 45.0m, Description = "Something", Id = commonId, Date = commonDate };

        Console.WriteLine(tr);
        Console.WriteLine(tr2);
        Console.WriteLine("Transactions are equal: {0}", tr2.Equals(tr));
    }
}
