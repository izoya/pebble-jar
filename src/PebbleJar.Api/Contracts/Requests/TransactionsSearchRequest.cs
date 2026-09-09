using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PebbleJar.Api.Contracts.Requests;

public class TransactionsSearchRequest : IValidatableObject
{
    [JsonIgnore]
    private const int DefaultPageSize = 50;

    public Guid? AccountId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? FromDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ToDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [StringLength(100)]
    public string? Query { get; init; } // description, rd.MerchantName, rd.Reference, rd.Particulars
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountFrom { get; init; }
    // TODO: Add absolute_amount_ filter
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountTo { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionType? TransactionType { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionCategory[]? Categories { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionKind[]? TransactionKinds { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionGrouping? Grouping { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TimeZoneId { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageNumber { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AmountFrom.HasValue && AmountTo.HasValue && AmountFrom > AmountTo)
        {
            yield return new ValidationResult(
                "AmountFrom must be less than or equal to AmountTo.",
                [nameof(AmountFrom), nameof(AmountTo)]);
        }

        if (!string.IsNullOrWhiteSpace(TimeZoneId))
        {
            ValidationResult? timeZoneValidationError = null;

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                timeZoneValidationError = new ValidationResult(
                    "Unknown time zone ID.",
                    [nameof(TimeZoneId)]);
            }
            catch (InvalidTimeZoneException)
            {
                timeZoneValidationError = new ValidationResult(
                    "Invalid time zone configuration.",
                    [nameof(TimeZoneId)]);
            }

            if (timeZoneValidationError is not null)
            {
                yield return timeZoneValidationError;
            }
        }
    }

    public TransactionQuery IntoQuery()
    {
        return new TransactionQuery(
            AccountId,
            FromDate,
            ToDate,
            AmountFrom,
            AmountTo,
            Query,
            TransactionType,
            Categories,
            TransactionKinds,
            Grouping,
            TimeZoneId,
            PageNumber ?? 1,
            PageSize ?? DefaultPageSize);
    }
}
