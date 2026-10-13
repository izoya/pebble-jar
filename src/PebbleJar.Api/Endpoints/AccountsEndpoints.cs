using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Api.Contracts.Responses;
using PebbleJar.Application.Interfaces;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;

namespace PebbleJar.Api.Endpoints;

public static class AccountsEndpoints
{
    public static IEndpointRouteBuilder MapAccountsEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/accounts", GetAccounts)
            .WithName("GetAccounts");

        endpoints.MapPatch("/accounts/{accountId:guid}", UpdateAccount)
            .WithName("UpdateAccount");

        endpoints.MapGet("/accounts/refresh", RefreshAccountsAsync)
            .WithName("RefreshAccounts")
            // Example of an endpoint-specific timeout
            .WithRequestTimeout(TimeSpan.FromSeconds(30));

        return endpoints;
    }

    public static async Task<Ok<IReadOnlyList<AccountListResponse>>> GetAccounts(
        IAccountRepository accounts,
        CancellationToken token)
    {
        var result = (await accounts.ListAsync(
            new AccountQuery(WithProvider: true),
            token))
            .Select(ToAccountListResponse)
            .ToList();

        return TypedResults.Ok<IReadOnlyList<AccountListResponse>>(result);
    }

    private static async Task<Results<NoContent, NotFound>> UpdateAccount(
        Guid accountId,
        UpdateAccountRequest request,
        IAccountRepository accounts,
        CancellationToken token)
    {
        var account = await accounts.GetByIdAsync(accountId, token);

        if (account is null) return TypedResults.NotFound();

        if (request.Name is { } name)
            account.Name = name;

        if (request.IsSyncEnabled is { } isSyncEnabled)
            account.SetSyncEnabled(isSyncEnabled);

        await accounts.UpdateAsync(account);

        return TypedResults.NoContent();
    }

    private static async Task<Ok<IReadOnlyList<AccountListResponse>>> RefreshAccountsAsync(
        IAccountSyncService syncService,
        IAccountRepository accounts,
        CancellationToken token)
    {
        await syncService.RefreshAsync(token);
        return await GetAccounts(accounts, token);
    }

    private static AccountListResponse ToAccountListResponse(Account account)
    {
        return new AccountListResponse(
            account.Id,
            account.Name,
            account.AccountNumber,
            account.FinancialInstitutionId,
            account.FinancialInstitution.Name,
            account.Currency,
            account.Status,
            account.IsSyncEnabled);
    }
}
