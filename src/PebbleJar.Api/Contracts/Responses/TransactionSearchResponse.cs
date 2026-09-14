using PebbleJar.Application.Queries;
using PebbleJar.Domain;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionSearchResponse(
    IReadOnlyList<TransactionItemResponse> Transactions,
    PageResponse Page,
    decimal TotalAmount,
    string TimeZoneId,
    int DataVersion,
    TransactionSearchParametersResponse Filters);

public sealed record TransactionGroupsSearchResponse(
    IReadOnlyList<TransactionGroupResponse> Groups,
    PageResponse Page,
    decimal TotalAmount,
    string TimeZoneId,
    int DataVersion,
    TransactionSearchParametersResponse Filters);



public sealed record TransactionGroupResponse(
    GroupingKey Key,
    decimal TotalAmount,
    int TotalCount);

public sealed record TransactionItemResponse(
    Guid Id,
    Guid AccountId,
    DateTimeOffset TransactionDateTimeUtc,
    DateTimeOffset TransactionDateTimeLocal,
    string? Description,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    TransactionKind Kind,
    TransactionRecognitionData? RecognitionData)
{
    public static TransactionItemResponse From(Transaction transaction)
    {
        var utcTimestamp = transaction.TransactionDateTime.ToUniversalTime();

        return new TransactionItemResponse(
            transaction.Id,
            transaction.AccountId,
            utcTimestamp,
            utcTimestamp.ToLocalTime(),
            transaction.Description,
            transaction.Amount,
            transaction.Type,
            transaction.Category,
            transaction.Kind,
            transaction.RecognitionData);
    }
}

