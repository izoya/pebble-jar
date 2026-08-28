using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Api.Endpoints;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using PebbleJar.Infrastructure;
using PebbleJar.Infrastructure.Akahu;
using PebbleJar.Infrastructure.Data;
using PebbleJar.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
# region Add services to the container 


//builder.Services.AddMemoryCache();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// http://localhost:5142/openapi/v1.json
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
});

builder.Services.AddDbContext<PebbleJarDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("PebbleJar"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IAccountRepository, SqliteAccountRepository>();
builder.Services.AddScoped<IFinancialInstitutionRepository, SqliteFinancialInstitutionRepository>();
builder.Services.AddScoped<ITransactionRepository, SqliteTransactionRepository>();
builder.Services.AddValidation();

builder.Services.AddAkahuService(builder.Configuration);

# endregion

var app = builder.Build();
app.UseRequestTimeouts();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => DateTime.UtcNow).WithName("Health");

app.MapAkahuAccountsEndpoints();

//app.MapGet("/accounts/{id:guid}", GetAccountById).WithName("GetAccountById");

app.MapGet("/transactions", GetTransactions).WithName("GetAllTransactions");
app.MapGet("/transactions/{id:guid}", GetTransactionById).WithName("GetTransactionById");
app.MapPost("/transactions", AddTransaction).WithName("AddTransaction");

app.MapPost("/seed", SeedData).WithName("SeedData");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/test-akahu", async (
        AkahuClient client,
        IOptions<AkahuOptions> options,
        ILogger<AkahuClient> logger,
        CancellationToken token) =>
    {
        return Results.Ok(new
        {
            ClientDump = client.ToString(),
            Options = options.Value,
            Response = await client.GetMeAsync(token)
        });
    });
}


# region Account Endpoints
//static async Task<Account?> GetAccountById(IAccountRepository repository, Guid id, CancellationToken token)
//{
//    return await repository.GetByIdAsync(id, token);
//}
# endregion

# region Transaction Endpoints
static async Task<IReadOnlyList<Transaction>> GetTransactions(ITransactionRepository repository)
{
    return await repository.ListAsync();
}
static async Task<Results<Ok<Transaction>, NotFound>> GetTransactionById(
    ITransactionRepository repository,
    Guid id)
{
    var transaction = await repository.GetByIdAsync(id);

    return transaction is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(transaction);
}

static async Task<Results<Ok, BadRequest<string>>> AddTransaction(ITransactionRepository repository, CreateTransactionRequest request)
{
    if (request.AccountId is not Guid accountId ||
        request.Amount is not decimal amount ||
        request.Category is not TransactionCategory category)
    {
        return TypedResults.BadRequest("Required fields are missing.");
    }

    var transaction = new Transaction
    {
        AccountId = accountId,
        Amount = amount,
        Category = category,
        Description = request.Description,
    };

    await repository.AddAsync(transaction);

    return TypedResults.Ok();
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
