using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using System.Text.Json.Serialization;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionSearchParametersResponse(
    TransactionFiltersResponse? Filters,
    int? PageNumber,
    int? PageSize,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    TransactionGrouping? Grouping)
{
    public static TransactionSearchParametersResponse From(
        TransactionSearchRequestBase request) => new(
        request.Filters is { } filters
            ? new TransactionFiltersResponse(
                filters.AccountId,
                filters.FromDate,
                filters.ToDate,
                filters.Query,
                filters.AmountFrom,
                filters.AmountTo,
                filters.TransactionType,
                filters.Categories,
                filters.TransactionKinds)
            : null,
        request.PageNumber,
        request.PageSize,
        (request as TransactionsSearchGroupRequest)?.Grouping);
}

public sealed record TransactionFiltersResponse(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Guid? AccountId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DateTimeOffset? FromDate,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DateTimeOffset? ToDate,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Query,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    decimal? AmountFrom,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    decimal? AmountTo,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    TransactionType? TransactionType,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    TransactionCategory[]? Categories,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    TransactionKind[]? TransactionKinds);
