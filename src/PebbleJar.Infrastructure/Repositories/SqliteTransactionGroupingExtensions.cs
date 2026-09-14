using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using System.Linq.Expressions;

namespace PebbleJar.Infrastructure.Repositories;

internal sealed class TimestampedTransaction
{
    public required decimal Amount { get; init; }
    public required TransactionCategory Category { get; init; }
    public required TransactionType Type { get; init; }
    public DateOnly? LocalDate { get; init; }
}

internal static class SqliteTransactionGroupingExtensions
{
    public static Expression<Func<TimestampedTransaction, string>> Apply(
        this TransactionGrouping grouping)
    {
        // Use this Monday day as starting point to calc 14-days intervals.
        var fortnightAnchor = new DateOnly(1970, 1, 5).DayNumber;

        return grouping switch
        {
            TransactionGrouping.TransactionCategory =>
                t => ((int)t.Category).ToString(),
            TransactionGrouping.TransactionType =>
                t => ((int)t.Type).ToString(),
            TransactionGrouping.Day =>
                t => t.LocalDate!.Value.DayNumber.ToString(),
            TransactionGrouping.Week =>
                t => t.LocalDate!.Value.AddDays(
                    -(((int)t.LocalDate.Value.DayOfWeek + 6) % 7)).DayNumber.ToString(),
            TransactionGrouping.Fortnight =>
                t => t.LocalDate!.Value.AddDays(
                    -(((t.LocalDate.Value.DayNumber - fortnightAnchor) % 14 + 14) % 14))
                    .DayNumber.ToString(),
            TransactionGrouping.Month =>
                t => t.LocalDate!.Value.AddDays(
                    1 - t.LocalDate.Value.Day).DayNumber.ToString(),
            TransactionGrouping.Year =>
                t => t.LocalDate!.Value.AddDays(
                    1 - t.LocalDate.Value.DayOfYear).DayNumber.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(grouping)),
        };
    }
}
