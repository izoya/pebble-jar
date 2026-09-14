using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using PebbleJar.Api.Endpoints;
using PebbleJar.Api.Serialization;
using PebbleJar.Application.Interfaces;
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
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(
        allowIntegerValues: false));
    options.SerializerOptions.Converters.Add(new GroupingKeyJsonConverter());
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
builder.Services.AddScoped<IDataVersionStore, SqliteDataVersionStore>();
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

app.MapAccountsEndpoints();
app.MapTransactionEndpoints();

app.Run();
