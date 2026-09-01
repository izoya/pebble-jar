using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

# region Services
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

#region App
var app = builder.Build();
app.UseRequestTimeouts();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => DateTime.UtcNow).WithName("Health");

app.MapAccountsEndpoints();
app.MapTransactionEndpoints();

app.MapPost("/seed", SeedData).WithName("SeedData");

#endregion

static async void SeedData(IAccountRepository accountRepository, ITransactionRepository transactionRepository)
{
    var (savingsAccount, testAccount) = Helpers.CreateAccounts();

    await accountRepository.AddAsync(savingsAccount);
    await accountRepository.AddAsync(testAccount);

    var transactions = new List<Transaction>
    {
        //new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 5, 5), Amount = -100m, Category = TransactionCategory.Utilities },
        //new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 6, 5), Amount = -50.25m, Description = "Groceries", Category = TransactionCategory.Food },
        //new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 7, 5), Amount = 20m, Description = "Refund", Category = TransactionCategory.Lifestyle },
        //new() { AccountId = savingsAccount.Id, Date = new DateTime(2026, 8, 5), Amount = -20m, Description = "To friends", Category = TransactionCategory.Lifestyle },
        //new() { AccountId = testAccount.Id, Date = new DateTime(2026, 6, 5), Amount = -450m, Description = "Power", Category = TransactionCategory.Utilities },
        //new() { AccountId = testAccount.Id, Date = new DateTime(2026, 8, 5), Amount = -45m, Description = "FreshChoice", Category = TransactionCategory.Food },
        //new() { AccountId = testAccount.Id, Date = new DateTime(2026, 8, 5), Amount = -1245m, Description = "Dentist", Category = TransactionCategory.Health },
        //new() { AccountId = testAccount.Id, Date = new DateTime(2026, 8, 5), Amount = 5310m, Description = "Salary" },
    };

    foreach (var transaction in transactions)
    {
        await transactionRepository.AddAsync(transaction);
    }
}

app.Run();
