using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Application.Queries;
using PebbleJar.Domain;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionSearchResponse(
    IReadOnlyList<TransactionItemResponse> Transactions,
    PageResponse Page,
    decimal TotalAmount,
    string TimeZoneId,
    int DataVersion,
    TransactionsSearchRequest Request);

public sealed record TransactionGroupsSearchResponse(
    IReadOnlyList<TransactionGroupResponse> Groups,
    PageResponse Page,
    decimal TotalAmount,
    string TimeZoneId,
    int DataVersion,
    TransactionsSearchGroupRequest Request);

public sealed record PageResponse(
    int Number,
    int Size,
    int TotalItems,
    int TotalPages,
    bool HasPrevious,
    bool HasNext);

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
    TransactionRecognitionData? RecognitionData);
