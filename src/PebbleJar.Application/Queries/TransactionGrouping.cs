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

/*
 Grouping	SQL expression
Day	        date(local_ts)

Week	    date(local_ts, '-' || ((CAST(strftime('%w', local_ts) AS INTEGER) + 6) % 7) || ' days')
            t.LocalTimestamp.Date.AddDays(
                -(((int)t.LocalTimestamp.DayOfWeek + 6) % 7)));

Fortnight	date('1970-01-05', printf('%+d days', CAST(floor((julianday(date(local_ts)) - julianday('1970-01-05')) / 14.0) AS INTEGER) * 14 ))
            var anchorDay = new DateOnly(1970, 1, 5).DayNumber;
            var grouped = qb.GroupBy(
                t => t.LocalTimestamp.Date.AddDays(
                    -(
                        (
                            (
                                DateOnly.FromDateTime(t.LocalTimestamp).DayNumber - anchorDay
                            ) % 14 + 14
                        ) % 14
                    )
                )
            );

Month	    date(local_ts, 'start of month')
            qb.GroupBy(t =>
                t.LocalTimestamp.Date.AddDays(1 - t.LocalTimestamp.Day));

Year	    date(local_ts, 'start of year')
            qb.GroupBy(t =>
                t.LocalTimestamp.Date.AddDays(1 - t.LocalTimestamp.DayOfYear));
 
 */

public abstract record GroupingKey
{
    public sealed record Date(DateTime Value) : GroupingKey;
    public sealed record Type(TransactionType Value) : GroupingKey;
    public sealed record Category(TransactionCategory Value) : GroupingKey;
}

public static class TransactionGroupingExtensions
{
    public static GroupingKey Apply(
        this TransactionGrouping grouping,
        Transaction transaction,
        DateTimeOffset localTimestamp)
    {
        GroupingKey x = grouping switch
        {
            TransactionGrouping.Day => new GroupingKey.Date(
                localTimestamp.Date),

            TransactionGrouping.Week => new GroupingKey.Date(
                localTimestamp.Date.AddDays(
                    -(((int)localTimestamp.DayOfWeek + 6) % 7))),

            TransactionGrouping.Fortnight => new GroupingKey.Date(
                calculateFortnight(localTimestamp)),

            TransactionGrouping.Month => new GroupingKey.Date(
                localTimestamp.Date.AddDays(1 - localTimestamp.Day)),

            TransactionGrouping.Year => new GroupingKey.Date(
                localTimestamp.Date.AddDays(1 - localTimestamp.DayOfYear)),

            TransactionGrouping.TransactionType => new GroupingKey.Type(
                transaction.Type),

            TransactionGrouping.TransactionCategory => new GroupingKey.Category(
                transaction.Category),

            _ => throw new NotSupportedException($"Grouping {grouping} is not supported"),
        };

        return x;
    }

    static DateTime calculateFortnight(DateTimeOffset localTimestamp)
    {
        var anchorDay = new DateOnly(1970, 1, 5).DayNumber;

        return localTimestamp.Date.AddDays(
            -((
                (DateOnly.FromDateTime(localTimestamp.Date).DayNumber - anchorDay) % 14 + 14
            ) % 14)
        );
    }
}
