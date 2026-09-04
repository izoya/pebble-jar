using PebbleJar.Application.Results;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Akahu.Mapping;
using PebbleJar.Infrastructure.Akahu.Models;
using Xunit;

namespace PebbleJar.Infrastructure.Tests.Akahu.Mapping;

public sealed class AkahuTransactionMapperTests
{
    private static readonly Guid DomainAccountId = Guid.NewGuid();

    [Fact]
    public void ToDomain_KnownTransaction_MapsFieldsAndReturnsComplete()
    {
        var source = CreateTransaction(
            AkahuTransactionType.Eftpos,
            category: CreateCategory("FOOD"),
            merchant: new AkahuMerchant { Name = "Corner shop" },
            meta: new AkahuTransactionMeta
            {
                CardSuffix = "1234",
            });

        var result = AkahuTransactionMapper.ToDomain(source, DomainAccountId);

        var complete = Assert.IsType<MappingResult<Transaction>.Complete>(result);
        Assert.Equal(source.Id, complete.Value.ExternalId);
        Assert.Equal(source.AccountId, complete.Value.ExternalAccountId);
        Assert.Equal(DomainAccountId, complete.Value.AccountId);
        Assert.Equal(source.Date, complete.Value.TransactionDateTime);
        Assert.Equal(source.Description, complete.Value.Description);
        Assert.Equal(source.Amount, complete.Value.Amount);
        Assert.Equal(TransactionKind.CardPayment, complete.Value.Kind);
        Assert.Equal(TransactionCategory.Food, complete.Value.Category);
        Assert.Equal("Corner shop", complete.Value.RecognitionData!.MerchantName);
        Assert.Equal("1234", complete.Value.RecognitionData.CardSuffix);
        Assert.NotNull(complete.Value.SourcePayloadJson);
        Assert.NotNull(complete.Value.SourceFetchedAt);
    }

    [Theory]
    [InlineData(AkahuTransactionType.Interest, TransactionCategory.InterestIncome)]
    [InlineData(AkahuTransactionType.Tax, TransactionCategory.Taxes)]
    public void ToDomain_InterestAndTax_UseTheirSpecialCategories(
        AkahuTransactionType type,
        TransactionCategory expectedCategory)
    {
        var source = CreateTransaction(type, category: CreateCategory("FOOD"));

        var result = AkahuTransactionMapper.ToDomain(source, DomainAccountId);

        var complete = Assert.IsType<MappingResult<Transaction>.Complete>(result);
        Assert.Equal(expectedCategory, complete.Value.Category);
    }

    [Fact]
    public void ToDomain_UnknownTransactionType_ReturnsPartialWithWarning()
    {
        var source = CreateTransaction((AkahuTransactionType)999);

        var result = AkahuTransactionMapper.ToDomain(source, DomainAccountId);

        var partial = Assert.IsType<MappingResult<Transaction>.Partial>(result);
        Assert.Equal(TransactionKind.Unspecified, partial.Value.Kind);

        var warnings = Assert.Single(partial.Warnings);
        Assert.Equal("type", warnings.Key);
        Assert.Contains("Unexpected Akahu transaction type value", warnings.Value.Single());
    }

    [Fact]
    public void ToDomain_MissingOptionalProviderData_UsesDefaults()
    {
        var source = CreateTransaction(AkahuTransactionType.Debit);

        var result = AkahuTransactionMapper.ToDomain(source, DomainAccountId);

        var complete = Assert.IsType<MappingResult<Transaction>.Complete>(result);
        Assert.Equal(TransactionKind.GenericDebit, complete.Value.Kind);
        Assert.Equal(TransactionCategory.Default, complete.Value.Category);
        Assert.NotNull(complete.Value.RecognitionData);
        Assert.Null(complete.Value.RecognitionData.MerchantName);
        Assert.Null(complete.Value.RecognitionData.Category);
        Assert.Null(complete.Value.RecognitionData.Code);
    }

    private static AkahuTransaction CreateTransaction(
        AkahuTransactionType type,
        AkahuTransactionCategory? category = null,
        AkahuMerchant? merchant = null,
        AkahuTransactionMeta? meta = null) =>
        new()
        {
            Id = "transaction-123",
            AccountId = "account-123",
            UserId = "user-123",
            ConnectionId = "connection-123",
            CreatedAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            Date = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Description = "Test transaction",
            Amount = -12.34m,
            Type = type,
            Category = category,
            Merchant = merchant,
            Meta = meta,
        };

    private static AkahuTransactionCategory CreateCategory(string groupName) =>
        new()
        {
            Name = "Provider category",
            Groups = new Dictionary<string, AkahuTransactionGroup>
            {
                ["personal_finance"] = new() { Name = groupName },
            },
        };
}
