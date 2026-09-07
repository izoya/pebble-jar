namespace PebbleJar.Api.Contracts.Responses;

public sealed record PagedResponse<T, R>(
    IReadOnlyList<T> Items,
    R Filters,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
