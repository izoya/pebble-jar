using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Application.Interfaces;
using PebbleJar.Infrastructure.Akahu;
using PebbleJar.Infrastructure.Akahu.Models;

namespace PebbleJar.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/accounts/{accountId:guid}/transactions", GetAccountTransactions)
            .WithName("Get Account Transactions");

        return endpoints;
    }

    private static async Task<Results<
        Ok<IReadOnlyList<AkahuTransaction>>,
        BadRequest<string>>> 
        GetAccountTransactions(
        Guid accountId,
        AkahuClient akahuClient,
        IAccountRepository accountsCtx,
        ITransactionRepository transactionsCtx,
        CancellationToken token)
    {
        var account = await accountsCtx.GetByIdAsync(accountId, token);

        // C#'s equivalent of Rust's `let Some() else {}`
        // Declared type's still Account?,
        // but its *flow state* after the guard is "known non-null"
        if (account is null)
        {
            return TypedResults.BadRequest("Requested account does not exist.");
        }

        Console.WriteLine($"Account found: {account.ExternalId}");

        // TODO: Remove hardcoded test values 
        var start = DateTimeOffset.UtcNow - TimeSpan.FromDays(5);

        var response = await akahuClient
            .ListAccountTransactionsPaginatedAsync(account.ExternalId, start, token: token);

        return TypedResults.Ok(response.Items);

        //throw new NotImplementedException("Unfinished endpoint");
    }
}
