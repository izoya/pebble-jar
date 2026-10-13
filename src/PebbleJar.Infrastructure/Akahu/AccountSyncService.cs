using PebbleJar.Application.Interfaces;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Akahu.Models;

namespace PebbleJar.Infrastructure.Akahu;

public sealed class AccountSyncService(
    AkahuClient akahuClient,
    IFinancialInstitutionRepository institutions,
    IAccountRepository accounts) : IAccountSyncService
{
    public async Task RefreshAsync(CancellationToken token)
    {
        var response = await akahuClient.ListAccountsAsync(token);
        var akahuAccountsByExternalId = response.Items
            .ToDictionary(acc => acc.Id);

        var storedAccounts = await accounts.ListAsync(new AccountQuery(), token);

        var institutionsByExternalId =
            await UpdateFinancialInstitutions(institutions, response, token);
        var knownAkahuAccountIds =
            await UpdateKnownAkahuAccounts(accounts, akahuAccountsByExternalId, storedAccounts);
        await InsertAkahuAccounts(
            accounts,
            response,
            institutionsByExternalId,
            knownAkahuAccountIds);
    }

    private static async Task InsertAkahuAccounts(
        IAccountRepository accounts,
        AkahuListResponse<AkahuAccount> response,
        Dictionary<string, FinancialInstitution> institutionsByExternalId,
        HashSet<string> knownAkahuAccountIds)
    {
        var accountsToAdd = response.Items
            .Where(account => !knownAkahuAccountIds.Contains(account.Id))
            .Select(account => ToAccount(account, institutionsByExternalId))
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
            .Select(account => new { account.Connection?.Id, account.Connection?.Name })
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
            .Distinct()
            .Where(x => !existingExternalIds.Contains(x.Id))
            .Select(x => new FinancialInstitution
            {
                Name = x.Name!,
                ExternalId = x.Id,
                ConnectionProvider = ConnectionProvider.Akahu
            })
            .ToList();

        if (missingInstitutions.Count > 0)
        {
            await institutions.AddManyAsync(missingInstitutions);
        }

        var institutionsByExternalId = existingInstitutions
            .Concat(missingInstitutions)
            .Where(institution => !string.IsNullOrWhiteSpace(institution.ExternalId))
            .ToDictionary(
                institution => institution.ExternalId!,
                StringComparer.Ordinal);

        return institutionsByExternalId;
    }

    private static Account ToAccount(
        AkahuAccount akahuAccount,
        Dictionary<string, FinancialInstitution> institutionsByExternalId)
    {
        var institutionExternalId = akahuAccount.Connection?.Id;

        if (string.IsNullOrWhiteSpace(institutionExternalId) ||
            !institutionsByExternalId.TryGetValue(
                institutionExternalId,
                out var institution))
        {
            throw new InvalidOperationException(
                $"Akahu account '{akahuAccount.Id}' has no matching financial institution.");
        }

        var account = new Account
        {
            Name = akahuAccount.Name ?? "Undefined",
            AccountNumber = akahuAccount.FormattedAccount,
            FinancialInstitutionId = institution.Id,
            ConnectionProvider = ConnectionProvider.Akahu,
            ExternalId = akahuAccount.Id,
            Status = ToAccountStatus(akahuAccount.Status),
            Currency = ToCurrency(akahuAccount.Balance?.Currency),
        };

        account.SetPayloadJson(akahuAccount);

        return account;
    }

    private static AccountStatus ToAccountStatus(AkahuAccountStatus? status) =>
        status switch
        {
            AkahuAccountStatus.Active => AccountStatus.Active,
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
