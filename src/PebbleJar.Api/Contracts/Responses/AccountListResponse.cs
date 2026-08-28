using PebbleJar.Domain;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record AccountListResponse(
    Guid Id,
    string Name,
    string? AccountNumber,
    Guid FinancialInstitutionId,
    string FinancialInstitutionName,
    Currency Currency,
    AccountStatus Status,
    bool IsSyncEnabled);
