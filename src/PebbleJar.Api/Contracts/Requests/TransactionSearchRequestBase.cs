using System.ComponentModel.DataAnnotations;
using PebbleJar.Application.Queries;

namespace PebbleJar.Api.Contracts.Requests;

public abstract class TransactionSearchRequestBase(int defaultPageSize)
    : IValidatableObject
{
    public TransactionsFilters? Filters { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageNumber { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => Filters?.Validate(validationContext) ?? [];

    public TransactionQuery ToQuery() => new(
        accountId: Filters?.AccountId,
        fromDate: Filters?.FromDate,
        toDate: Filters?.ToDate,
        amountFrom: Filters?.AmountFrom,
        amountTo: Filters?.AmountTo,
        query: Filters?.Query,
        transactionType: Filters?.TransactionType,
        categoryIds: Filters?.Categories,
        transactionKindIds: Filters?.TransactionKinds,
        pageNumber: PageNumber ?? 1,
        pageSize: PageSize ?? defaultPageSize);
}
