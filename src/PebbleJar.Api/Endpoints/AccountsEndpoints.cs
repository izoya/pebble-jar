using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Infrastructure.Akahu;
using PebbleJar.Infrastructure.Akahu.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PebbleJar.Api.Endpoints;

public static class AccountsEndpoints
{
    public static IEndpointRouteBuilder MapAkahuAccountsEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/akahu/accounts", ListAccountsAsync)
            .WithName("ListAkahuAccounts");

        return endpoints;
    }

    private static async Task<Ok<AkahuListResponse<AkahuAccount>>> ListAccountsAsync(
        AkahuClient akahuClient,
        CancellationToken cancellationToken)
    {
        var response = await akahuClient.ListAccountsAsync(cancellationToken);

        return TypedResults.Ok(response);
    }
}
