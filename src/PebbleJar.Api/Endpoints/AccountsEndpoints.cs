using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Api.Contracts.Responses;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Akahu;
using PebbleJar.Infrastructure.Akahu.Models;

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
        var result = (await accounts.ListAsync(token))
            .Select(ToAccountListResponse())
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
        AkahuClient akahuClient,
        IFinancialInstitutionRepository institutions,
        IAccountRepository accounts,
        CancellationToken token)
    {
        var response = await akahuClient.ListAccountsAsync(token);
        var akahuAccountsByExternalId = response.Items
            .ToDictionary(acc => acc.Id);

        var storedAccounts = await accounts.ListAsync(token);

        var institutionsByName =
            await UpdateFinancialInstitutions(institutions, response, token);
        var knownAkahuAccountIds =
            await UpdateKnownAkahuAccounts(accounts, akahuAccountsByExternalId, storedAccounts);
        await InsertAkahuAccounts(accounts, response, institutionsByName, knownAkahuAccountIds);

        var institutionsById = institutionsByName.Values
            .ToDictionary(institution => institution.Id);

        var reviewAccounts = (await accounts.ListAsync(token))
            .Select(ToAccountListResponse())
            .ToList();

        return TypedResults.Ok<IReadOnlyList<AccountListResponse>>(reviewAccounts);
    }

    private static Func<Account, AccountListResponse> ToAccountListResponse()
    {
        return account => new AccountListResponse(
            account.Id,
            account.Name,
            account.AccountNumber,
            account.FinancialInstitutionId,
            account.FinancialInstitution.Name,
            account.Currency,
            account.Status,
            account.IsSyncEnabled);
    }

    private static async Task InsertAkahuAccounts(IAccountRepository accounts, AkahuListResponse<AkahuAccount> response, Dictionary<string, FinancialInstitution> institutionsByName, HashSet<string> knownAkahuAccountIds)
    {
        var accountsToAdd = response.Items
            .Where(account => !knownAkahuAccountIds.Contains(account.Id))
            .Select(account => ToAccount(account, institutionsByName))
            .ToList();

        if (accountsToAdd.Count > 0)
        {
            await accounts.AddManyAsync(accountsToAdd);
        }
    }

    private static async Task<HashSet<string>> UpdateKnownAkahuAccounts(
        IAccountRepository accounts,
        Dictionary<string, AkahuAccount> akahuAccountsByExternalId,
        IReadOnlyList<Account> storedAccounts)
    {
        var akahuAccountsToUpdate = storedAccounts
            .Where(account => account.ConnectionProvider == ConnectionProvider.Akahu)
            .Where(account => account.ExternalId is not null
                // Ensure the account we stored is not stale (still present in Akahu response)
                && akahuAccountsByExternalId.ContainsKey(account.ExternalId))
            .ToList();

        foreach (var account in akahuAccountsToUpdate)
        {
            account.SetPayloadJson(
                akahuAccountsByExternalId[account.ExternalId!]);
        }

        await accounts.UpdateManyAsync(akahuAccountsToUpdate);

        var knownAkahuAccountIds = akahuAccountsToUpdate
            .Select(account => account.ExternalId)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
        return knownAkahuAccountIds;
    }

    private static async Task<Dictionary<string, FinancialInstitution>> UpdateFinancialInstitutions(
        IFinancialInstitutionRepository institutions,
        AkahuListResponse<AkahuAccount> response,
        CancellationToken token)
    {
        var existingInstitutions = await institutions.ListAsync(token);
        var existingExternalIds = existingInstitutions
            .Select(institution => institution.ExternalId)
            .ToHashSet(StringComparer.Ordinal);

        var missingInstitutions = response.Items
            .Select(account => new { Id = account.Connection?.Id, Name = account.Connection?.Name })
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
            .Distinct()
            .Where(x => !existingExternalIds.Contains(x.Id))
            .Select(x => new FinancialInstitution
            {
                Name = x.Name,
                ExternalId = x.Id,
                ConnectionProvider = ConnectionProvider.Akahu
            })
            .ToList();

        if (missingInstitutions.Count > 0)
        {
            await institutions.AddManyAsync(missingInstitutions);
        }

        var institutionsByName = existingInstitutions
            .Concat(missingInstitutions)
            .ToDictionary(
                institution => institution.Name,
                StringComparer.OrdinalIgnoreCase);

        return institutionsByName;
    }

    private static Account ToAccount(
        AkahuAccount AkahuAccount,
        Dictionary<string, FinancialInstitution> InstitutionsByName)
    {
        var institutionName = AkahuAccount.Connection?.Name;

        if (string.IsNullOrWhiteSpace(institutionName) ||
            !InstitutionsByName.TryGetValue(institutionName, out var institution))
        {
            throw new InvalidOperationException(
                $"Akahu AkahuAccount '{AkahuAccount.Id}' has no matching financial institution.");
        }

        var account = new Account
        {
            Name = AkahuAccount.Name ?? "Undefined",
            AccountNumber = AkahuAccount.FormattedAccount,
            FinancialInstitutionId = institution.Id,
            ConnectionProvider = ConnectionProvider.Akahu,
            ExternalId = AkahuAccount.Id,
            Status = ToAccountStatus(AkahuAccount.Status),
            Currency = ToCurrency(AkahuAccount.Balance?.Currency),
        };

        account.SetPayloadJson(AkahuAccount);

        return account;
    }

    private static AccountStatus ToAccountStatus(AkahuAccountStatus? status) =>
        status switch
        {
            AkahuAccountStatus.Active => AccountStatus.Active,
            AkahuAccountStatus.Inactive => AccountStatus.Inactive,
            _ => AccountStatus.Inactive,
        };

    private static Currency ToCurrency(string? currency) =>
        currency?.ToUpper() switch
        {
            "NZD" => Currency.NZD,
            "USD" => Currency.USD,
            _ => throw new InvalidOperationException(
                $"Unsupported account currency '{currency ?? "missing"}'."),
        };
}
