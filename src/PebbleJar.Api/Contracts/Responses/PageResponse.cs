using PebbleJar.Application.Results;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record PageResponse(
    int Number,
    int Size,
    int TotalItems,
    int TotalPages,
    bool HasPrevious,
    bool HasNext)
{

    public static PageResponse From(Pagination page) => new(
        page.PageNumber,
        page.PageSize,
        page.TotalCount,
        page.TotalPages,
        page.HasPreviousPage,
        page.HasNextPage);
}
