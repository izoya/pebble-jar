using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Api.Contracts.Responses;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using Xunit;

namespace PebbleJar.Api.Tests;

public sealed class TransactionSearchValidationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public TransactionSearchValidationTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders().AddConsole());
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PebbleJar"] = "Data Source=:memory:",
                    ["Akahu:AppIdToken"] = "test-app-token",
                    ["Akahu:UserAccessToken"] = "test-user-token",
                }));
        });
    }

    [Theory]
    [InlineData("/transactions/search")]
    [InlineData("/transactions/search/groups")]
    public async Task Search_ValidatesNestedFilterAttributesAndCustomRules(string endpoint)
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });
        var cases = new (object Filters, string[] Members)[]
        {
            (new { query = new string('x', 101) }, ["Query"]),
            (new { from_date = "2026-01-02T00:00:00Z", to_date = "2026-01-01T00:00:00Z" },
                ["FromDate", "ToDate"]),
            (new { amount_from = 20m, amount_to = 10m }, ["AmountFrom", "AmountTo"]),
        };

        foreach (var (filters, members) in cases)
        {
            using var response = await client.PostAsJsonAsync(endpoint, new { filters, grouping = "Day" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = Assert.IsType<HttpValidationProblemDetails>(
                await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>());
            Assert.Equal(members.Length, problem.Errors.Count);
            foreach (var member in members)
                Assert.Single(problem.Errors[$"Filters.{member}"]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EchoedParameters_PreserveValuesAndOmitNullFiltersOnly(bool grouped)
    {
        var options = factory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
        TransactionSearchBaseRequest CreateRequest(TransactionsFilters filters) => grouped
            ? new TransactionsSearchGroupRequest
            {
                Filters = filters, PageNumber = 2, PageSize = 10, Grouping = TransactionGrouping.Month,
            }
            : new TransactionsSearchRequest { Filters = filters, PageNumber = 2, PageSize = 10 };

        var request = CreateRequest(new TransactionsFilters
        {
            AccountId = Guid.NewGuid(),
            FromDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(13)),
            ToDate = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.FromHours(13)),
            Query = "coffee",
            AmountFrom = 0m,
            AmountTo = 100m,
            TransactionType = TransactionType.Debit,
            Categories = [TransactionCategory.Food],
            TransactionKinds = [TransactionKind.CardPayment],
        });
        Assert.True(JsonNode.DeepEquals(
            JsonSerializer.SerializeToNode(request, request.GetType(), options),
            JsonSerializer.SerializeToNode(TransactionSearchParametersResponse.From(request), options)));

        var partialRequest = CreateRequest(new TransactionsFilters { Query = "coffee" });
        var echo = JsonSerializer.SerializeToNode(TransactionSearchParametersResponse.From(partialRequest), options)!;
        Assert.Single(echo["filters"]!.AsObject());
        Assert.Equal("coffee", echo["filters"]!["query"]!.GetValue<string>());
        Assert.Equal(grouped, echo.AsObject().ContainsKey("grouping"));
        Assert.Equal(JsonIgnoreCondition.Never, options.DefaultIgnoreCondition);
    }
}
