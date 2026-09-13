using PebbleJar.Domain;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PebbleJar.Api.Contracts.Requests;

public class TransactionsFilters : IValidatableObject
{
    public Guid? AccountId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    // Date filters represent absolute instants. 
    // Date filtering and grouping use the host's local timezone.
    public DateTimeOffset? FromDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ToDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [StringLength(100)]
    /// Pattern search in Description, MerchantName, Reference, Particulars
    public string? Query { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountFrom { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountTo { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionType? TransactionType { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionCategory[]? Categories { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
