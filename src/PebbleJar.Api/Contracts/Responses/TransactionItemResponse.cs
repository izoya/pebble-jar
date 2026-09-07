using PebbleJar.Domain;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionItemResponse(
    Guid Id,
    Guid AccountId,
    DateTimeOffset TransactionDateTime,
    string? Description,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    TransactionKind Kind,
    TransactionRecognitionData? RecognitionData);
