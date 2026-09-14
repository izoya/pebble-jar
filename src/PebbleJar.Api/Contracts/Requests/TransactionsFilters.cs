using PebbleJar.Domain;
using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Api.Contracts.Requests;

public class TransactionsFilters : IValidatableObject
{
    public Guid? AccountId { get; init; }

    // Date filters represent absolute instants. 
    // Date filtering and grouping use the host's local timezone.
    public DateTimeOffset? FromDate { get; init; }

    public DateTimeOffset? ToDate { get; init; }

    // Pattern search in Description, MerchantName, Reference, Particulars
    // 100 should be plenty as various descriptions are often capped at ~40 characters
    [StringLength(100)]
    public string? Query { get; init; }

    public decimal? AmountFrom { get; init; }

    public decimal? AmountTo { get; init; }

    public TransactionType? TransactionType { get; init; }

    public TransactionCategory[]? Categories { get; init; }

    public TransactionKind[]? TransactionKinds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate is { } from && ToDate is { } to && from > to)
            yield return new ValidationResult("FromDate must be earlier than or equal to ToDate.",
                [nameof(FromDate), nameof(ToDate)]);

        if (AmountFrom is { } amountFrom && AmountTo is { } amountTo && amountFrom > amountTo)
        {
            yield return new ValidationResult(
                "AmountFrom must be less than or equal to AmountTo.",
                [nameof(AmountFrom), nameof(AmountTo)]);
        }

    }
}
