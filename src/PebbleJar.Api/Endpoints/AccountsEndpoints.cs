using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Api.Contracts.Responses;
using PebbleJar.Application.Interfaces;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Akahu;
using PebbleJar.Infrastructure.Akahu.Models;

namespace PebbleJar.Api.Endpoints;

public static class AccountsEndpoints
{
    public static IEndpointRouteBuilder MapAkahuAccountsEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/accounts/refresh", RefreshAccountsAsync)
            .WithName("RefreshAccounts");

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<AccountReviewResponse>>> RefreshAccountsAsync(
        AkahuClient akahuClient,
        IFinancialInstitutionRepository institutions,
        IAccountRepository accounts,
        CancellationToken token)
    {
        var response = await akahuClient.ListAccountsAsync(token);

        var existingInstitutions = await institutions.ListAsync(token);
        var existingNames = existingInstitutions
            .Select(institution => institution.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingInstitutions = response.Items
            .Select(account => account.Connection?.Name)
            .OfType<string>()
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => !existingNames.Contains(name))
            .Select(name => new FinancialInstitution { Name = name })
            .ToList();

        if (missingInstitutions.Count > 0)
        {
            await institutions.AddManyAsync(missingInstitutions, token);
        }

        var institutionsByName = existingInstitutions
            .Concat(missingInstitutions)
            .ToDictionary(
                institution => institution.Name,
                StringComparer.OrdinalIgnoreCase);

        var storedAccounts = await accounts.ListAsync(token);
        var knownAkahuAccountIds = storedAccounts
            .Where(account => account.ConnectionProvider == ConnectionProvider.Akahu)
            .Select(account => account.ExternalId)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        var accountsToAdd = response.Items
            .Where(account => knownAkahuAccountIds.Add(account.Id))
            .Select(account => ToAccount(account, institutionsByName))
            .ToList();

        if (accountsToAdd.Count > 0)
        {
            await accounts.AddManyAsync(accountsToAdd);
        }

        var institutionsById = institutionsByName.Values
            .ToDictionary(institution => institution.Id);

        var reviewAccounts = (await accounts.ListAsync(token))
            .Select(account => new AccountReviewResponse(
                account.Id,
                account.Name,
                account.AccountNumber,
                account.FinancialInstitutionId,
                institutionsById[account.FinancialInstitutionId].Name,
                account.Currency,
                account.Status,
                account.IsSyncEnabled))
            .ToList();

        return TypedResults.Ok<IReadOnlyList<AccountReviewResponse>>(reviewAccounts);
    }

    private static Account ToAccount(
        AkahuAccount account,
        IReadOnlyDictionary<string, FinancialInstitution> institutionsByName)
    {
        var institutionName = account.Connection?.Name;

        if (string.IsNullOrWhiteSpace(institutionName) ||
            !institutionsByName.TryGetValue(institutionName, out var institution))
        {
            throw new InvalidOperationException(
                $"Akahu account '{account.Id}' has no matching financial institution.");
        }

        return new Account
        {
            Name = account.Name ?? "Undefined",
            AccountNumber = account.FormattedAccount,
            FinancialInstitutionId = institution.Id,
            ConnectionProvider = ConnectionProvider.Akahu,
            ExternalId = account.Id,
            Status = ToAccountStatus(account.Status),
            Currency = ToCurrency(account.Balance?.Currency),
        };
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
