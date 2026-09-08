using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionSearchResponse(
    PagedResponse<TransactionItemResponse, TransactionsSearchRequest>? Transactions,
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
    GroupingKey Key,
    IReadOnlyList<TransactionItemResponse> Transactions,
    decimal TotalAmount,
    int TotalCount);
