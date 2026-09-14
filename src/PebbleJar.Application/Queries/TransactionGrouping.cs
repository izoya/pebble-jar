using OneOf;
using PebbleJar.Domain;

namespace PebbleJar.Application.Queries;

public enum TransactionGrouping
{
    Day,
    Week,
    Fortnight,
    Month,
    Year,
    TransactionType,
    TransactionCategory,
}

public record GroupingKey(
    TransactionGrouping Key,
    OneOf<DateTime, TransactionType, TransactionCategory> Value);
