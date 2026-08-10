using PebbleJar.Domain;
using PebbleJar.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/health", () =>
{
    Tmp();
    return DateTime.Now;
})
.WithName("Health");





app.Run();


static void Tmp()
{
    var account = new Account
    {
        Name = "Savings",
        AccountNumber = "12345",
        Currency = PebbleJar.Domain.Currency.NZD,
    };
    Console.WriteLine($"{account}\n");

    var transactions = new List<Transaction> {
            new Transaction { AccountId = account.Id, Date = new DateTime(2026, 5, 5), Amount = -100m, Category = TransactionCategory.Utilities },
            new Transaction { AccountId = account.Id, Date = new DateTime(2026, 6, 5), Amount = -50.25m, Description = "Groceries", Category = TransactionCategory.Groceries },
            new Transaction { AccountId = account.Id, Date = new DateTime(2026, 7, 5), Amount = 20m, Description = "Refund", Category = TransactionCategory.Entertainment },
            new Transaction { AccountId = account.Id, Date = new DateTime(2026, 8, 5), Amount = -20m, Description = "To friends", Category = TransactionCategory.Entertainment },

        };


    var account2 = new Account() { AccountNumber = "fff", Name = "Test", Currency = Currency.NZD };
    var transactions2 = new List<Transaction>
        {
            new() { AccountId = account2.Id, Date = new DateTime(2026, 6, 5), Amount = -450m, Description = "Power", Category = TransactionCategory.Utilities },
            new() { AccountId = account2.Id, Date = new DateTime(2026, 8, 5), Amount = -45m, Description = "FreshChoice", Category = TransactionCategory.Groceries },
            new() { AccountId = account2.Id, Date = new DateTime(2026, 8, 5), Amount = -1245m, Description = "Dentist", Category = TransactionCategory.Healthcare },
            new() { AccountId = account2.Id, Date = new DateTime(2026, 8, 5), Amount = 5310m, Description = "Salary" },
        };

    var allTransactions = transactions.Union(transactions2);

    var credited = allTransactions.Where(t => t.Amount > 0).OrderBy(t => t.Amount);
    Console.WriteLine($"--------------------------\nCredited {credited.Count()} transactions: \n--------------------------");
    credited.Dump();


    var date = new DateOnly(2026, 8, 5);
    var monthlyExpenses = allTransactions
        .Where(t => t.Type == TransactionType.Debit)
        .Where(t => t.Date.Year == date.Year && t.Date.Month == date.Month)
        .Select(t => t);
    Console.WriteLine($"--------------------------\nExpenses in {date.Month}/{date.Year}: \n--------------------------");
    monthlyExpenses.Dump();

    var totalMonthlyExpenses = monthlyExpenses.Sum(t => t.Amount);
    Console.WriteLine($"--------------------------\nTotal monthly expenses: {Math.Abs(totalMonthlyExpenses):C2}\n--------------------------");

    var expensesByCategory = allTransactions
        .GroupBy(t => t.Category)
        .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) });

    foreach (var group in expensesByCategory)
    {
        Console.WriteLine($"{group.Category}, Total: {Math.Abs(group.Total):C2}");
    }


    // Create a LINQ query, mutate the source list, then enumerate the query. Record what happened and why.

    // Rewrite one LINQ expression as a normal loop and compare readability.

}