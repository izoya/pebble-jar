using PebbleJar.Api.Contracts.Requests;

namespace PebbleJar.Api.Contracts.Responses;

public sealed record TransactionSearchResponse(
    PagedResponse<TransactionItemResponse, TransactionsListRequest> Transactions,
    decimal TotalAmount);
