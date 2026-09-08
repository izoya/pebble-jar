using Microsoft.AspNetCore.Http.HttpResults;
using PebbleJar.Api.Contracts.Requests;
using PebbleJar.Api.Contracts.Responses;
using PebbleJar.Application.Interfaces;
using PebbleJar.Application.Queries;
using PebbleJar.Application.Results;
using PebbleJar.Domain;
using PebbleJar.Extensions;
using PebbleJar.Infrastructure.Akahu;
using PebbleJar.Infrastructure.Akahu.Mapping;
using PebbleJar.Infrastructure.Akahu.Models;

namespace PebbleJar.Api.Endpoints;

public static class TransactionEndpoints
{
    private const string LogCategory = nameof(TransactionEndpoints);

    public static IEndpointRouteBuilder MapTransactionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/transactions/search", SearchTransactions)
            .WithName("Get Account Transactions");

        endpoints.MapPost("/transactions/sync", SyncTransactions)
            .WithName("Sync Account Transactions");

        return endpoints;
    }

    private static async Task<Results<
        Ok<TransactionSearchResponse>,
        BadRequest<string>,
        NotFound<string>>>
        SearchTransactions(
        TransactionsSearchRequest request,
        IAccountRepository accountsCtx,
        ITransactionRepository transactionsCtx,
        ILoggerFactory loggerFactory,
        CancellationToken token)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);

        if (request.AccountId is { } accountId
            && await accountsCtx.GetByIdAsync(accountId, token) is null)
        {
            return TypedResults.NotFound("Requested account does not exist.");
        }

        var query = request.IntoQuery();

        var result = await transactionsCtx.ListAsync(query, token);

        TransactionItemResponse MapTransaction(Transaction transaction) =>
            ToTransactionListResponse(transaction, query.TimeZone);

        return TypedResults.Ok(result switch
        {
            UngroupedTransactionQueryResult ungrouped => new TransactionSearchResponse(
                ToPage(ungrouped.Transactions, request, MapTransaction),
                null,
                ungrouped.TotalAmount,
                query.TimeZoneId),
            GroupedTransactionQueryResult grouped => new TransactionSearchResponse(
                null,
                ToPage(grouped.Groups, request, group => new GroupedTransactionResponse(
                    group.Key,
                    group.Transactions.Select(MapTransaction).ToList(),
                    group.TotalAmount,
                    group.TotalCount)),
                grouped.TotalAmount,
                query.TimeZoneId),
            _ => throw new InvalidOperationException("Unknown transaction query result."),
        });

    }


    private static async Task<Results<
        Ok,
        BadRequest<string>,
        NotFound<string>>>
        SyncTransactions(
        SyncTransactionsRequest request,
        AkahuClient akahuClient,
        IAccountRepository accountsCtx,
        ITransactionRepository transactionsCtx,
        ILoggerFactory loggerFactory,
        CancellationToken token)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);

        List<Account> accountsToSync = [];

        if (request.AccountId is { } accountId)
        {
            if (await accountsCtx.GetByIdAsync(accountId, token) is not { } account)
            {
                return TypedResults.NotFound("Requested account does not exist.");
            }

            accountsToSync = [account];
        }
        else
        {
            accountsToSync = [.. (await accountsCtx
                .ListAsync(new AccountQuery(IsSyncEnabled: true), token))];
        }

        async Task SyncAccountTransactionsAsync(string accountExternalId, Guid accountId)
        {
            var start = request.FromDate;

            if (start is null)
            {
                var latestTransactionDate = await transactionsCtx
                    .GetLatestTransactionDateAsync(accountId, token);

                if (latestTransactionDate is { } latest)
                    start = latest - TimeSpan.FromDays(7);
            }

            // At this point, if start is still null, this means it's an initial sync.
            // Forcibly limit initial sync to a ~3 months period.
            start ??= DateTimeOffset.UtcNow - TimeSpan.FromDays(90);

            string? cursor = null;

            do
            {
                var response = await akahuClient.ListAccountTransactionsPaginatedAsync(
                    accountExternalId,
                    start,
                    cursor: cursor,
                    token: token);

                var transactions = response.Items
                    .Select(transaction => ToDomainTransaction(transaction, accountId, logger))
                    .ToList();

                await transactionsCtx.AddMissingAsync(accountId, transactions, token);
                cursor = response.Cursor?.Next;
            } while (cursor is not null);
        }

        foreach (var acc in accountsToSync)
        {
            if (acc.ExternalId is { } accountExternalId)
            {
                await SyncAccountTransactionsAsync(accountExternalId, acc.Id);
            }

        }

        return TypedResults.Ok();
    }


    private static TransactionItemResponse ToTransactionListResponse(
        Transaction transaction,
        TimeZoneInfo timeZone)
    {
        var utcTimestamp = transaction.TransactionDateTime.ToUniversalTime();

        return new TransactionItemResponse(
            transaction.Id,
            transaction.AccountId,
            utcTimestamp,
            TimeZoneInfo.ConvertTime(utcTimestamp, timeZone),
            transaction.Description,
            transaction.Amount,
            transaction.Type,
            transaction.Category,
            transaction.Kind,
            transaction.RecognitionData);
    }

    private static PagedResponse<TResponse, TransactionsSearchRequest> ToPage<TItem, TResponse>(
        PagedResult<TItem> page,
        TransactionsSearchRequest request,
        Func<TItem, TResponse> map)
    {
        return new PagedResponse<TResponse, TransactionsSearchRequest>(
            page.Items.Select(map).ToList(),
            request,
            page.PageNumber,
            page.PageSize,
            page.TotalCount,
            page.TotalPages,
            page.HasPreviousPage,
            page.HasNextPage);
    }

    private static Transaction ToDomainTransaction(
        AkahuTransaction transaction,
        Guid accountId,
        ILogger logger)
    {
        var mapping = AkahuTransactionMapper.ToDomain(transaction, accountId);

        Transaction ProcessPartialResult(MappingResult<Transaction>.Partial result)
        {
            logger.LogWarning(
                "Partial failure in Akahu Transaction mapping: {Warnings}",
                result.Warnings.ToDebugString());

            return result.Value;
        }

        var domainTransaction = mapping switch
        {
            MappingResult<Transaction>.Complete result => result.Value,
            MappingResult<Transaction>.Partial result => ProcessPartialResult(result),
            MappingResult<Transaction>.Failed result => throw new InvalidOperationException(
                $"Could not map Akahu transaction {transaction.Id}: {result.Errors.ToDebugString()}"),
            _ => throw new InvalidOperationException("Unknown transaction mapping result."),
        };

        return domainTransaction;
    }
}
