using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using PebbleJar.Infrastructure;
using PebbleJar.Infrastructure.Repositories;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
# region Add services to the container 

// InMemory repositories registered as singletons ensure the inner storage persists across requests.
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
//builder.Services.AddMemoryCache();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// http://localhost:5142/openapi/v1.json
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.SnakeCaseLower;
});
# endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => DateTime.UtcNow).WithName("Health");

app.MapGet("/accounts", GetAccounts).WithName("GetAllAccounts");
app.MapGet("/accounts/{id:guid}", GetAccountById).WithName("GetAccountById");
app.MapPost("/accounts", AddAccount).WithName("AddAccount");

app.MapGet("/transactions", GetTransactions).WithName("GetAllTransactions");
app.MapGet("/transactions/{id:guid}", GetTransactionById).WithName("GetTransactionById");
app.MapPost("/transactions", AddTransaction).WithName("AddTransaction");

app.MapPost("/seed", SeedData).WithName("SeedData");


# region Account Endpoints
static async Task<IReadOnlyList<Account>> GetAccounts(IAccountRepository repository)
{
    return await repository.ListAsync();
}
static async Task<Account?> GetAccountById(IAccountRepository repository, Guid id)
{
    return await repository.GetByIdAsync(id);
}
static async Task AddAccount(IAccountRepository repository, Account account)
{
    await repository.AddAsync(account);
}
# endregion

# region Transaction Endpoints
static async Task<IReadOnlyList<Transaction>> GetTransactions(ITransactionRepository repository)
{
    return await repository.ListAsync();
}
static async Task<Results<Ok<Transaction>, NotFound>> GetTransactionById(ITransactionRepository repository, Guid id)
{
    var transaction = await repository.GetByIdAsync(id);

    if (transaction is null) return TypedResults.NotFound();

    return TypedResults.Ok(transaction);
}
static async Task AddTransaction(ITransactionRepository repository, Transaction transaction)
{
    await repository.AddAsync(transaction);
}
# endregion

static async void SeedData(IAccountRepository accountRepository, ITransactionRepository transactionRepository)
{
    var (savingsAccount, testAccount) = Helpers.CreateAccounts();

    await accountRepository.AddAsync(savingsAccount);
    await accountRepository.AddAsync(testAccount);

    var transactions = new List<Transaction>
    {
        new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 5, 5), Amount = -100m, Category = TransactionCategory.Utilities },
        new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 6, 5), Amount = -50.25m, Description = "Groceries", Category = TransactionCategory.Groceries },
        new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 7, 5), Amount = 20m, Description = "Refund", Category = TransactionCategory.Entertainment },
        new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 8, 5), Amount = -20m, Description = "To friends", Category = TransactionCategory.Entertainment },
        new() { AccountId = testAccount.Id, Date = new DateTime(2026, 6, 5), Amount = -450m, Description = "Power", Category = TransactionCategory.Utilities },
        new() { AccountId = testAccount.Id, Date = new DateTime(2026, 8, 5), Amount = -45m, Description = "FreshChoice", Category = TransactionCategory.Groceries },
        new() { AccountId = testAccount.Id, Date = new DateTime(2026, 8, 5), Amount = -1245m, Description = "Dentist", Category = TransactionCategory.Healthcare },
        new() { AccountId = testAccount.Id, Date = new DateTime(2026, 8, 5), Amount = 5310m, Description = "Salary" },
    };

    foreach (var transaction in transactions)
    {
        await transactionRepository.AddAsync(transaction);
    }
}

app.Run();


static void Tmp()
{
    //var credited = allTransactions.Where(t => t.Amount > 0).OrderBy(t => t.Amount);
    //Console.WriteLine($"--------------------------\nCredited {credited.Count()} transactions: \n--------------------------");
    //credited.Dump();

    //var date = new DateOnly(2026, 8, 5);
    //var monthlyExpenses = allTransactions
    //    .Where(t => t.Type == TransactionType.Debit)
    //    .Where(t => t.Date.Year == date.Year && t.Date.Month == date.Month)
    //    .Select(t => t);
    //Console.WriteLine($"--------------------------\nExpenses in {date.Month}/{date.Year}: \n--------------------------");
    //monthlyExpenses.Dump();

    //var totalMonthlyExpenses = monthlyExpenses.Sum(t => t.Amount);
    //Console.WriteLine($"--------------------------\nTotal monthly expenses: {Math.Abs(totalMonthlyExpenses):C2}\n--------------------------");

    //var expensesByCategory = allTransactions
    //    .GroupBy(t => t.Category)
    //    .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) });

    //foreach (var group in expensesByCategory)
    //{
    //    Console.WriteLine($"{group.Category}, Total: {Math.Abs(group.Total):C2}");
    //}
}
