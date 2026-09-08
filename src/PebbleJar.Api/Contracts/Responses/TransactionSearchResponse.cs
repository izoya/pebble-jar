using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Domain;
using System.Text.Json.Serialization;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionSearchResponse(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    PagedResponse<TransactionItemResponse, TransactionsSearchRequest>? Transactions,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    PagedResponse<GroupedTransactionResponse, TransactionsSearchRequest>? Groups,
    decimal TotalAmount,
    string TimeZoneId);

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
    TransactionRecognitionData? RecognitionData);

public sealed record GroupedTransactionResponse(
    GroupingKeyResponse Key,
    IReadOnlyList<TransactionItemResponse> Transactions,
    decimal TotalAmount,
    int TotalCount);

public sealed record GroupingKeyResponse(
    string Type,
    string Value);
