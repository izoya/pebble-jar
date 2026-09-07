using PebbleJar.Application.Results;
using PebbleJar.Domain;
using PebbleJar.Infrastructure.Akahu.Models;

namespace PebbleJar.Infrastructure.Akahu.Mapping;

public static class AkahuTransactionMapper
{
    private const string AkahuDefaultTransactionsGroupKey = "personal_finance";

    public static MappingResult<Transaction> ToDomain(
        AkahuTransaction source,
        Guid domainAccountId)
    {
        var warnings = new Dictionary<string, List<string>>();

        var transaction = new Transaction
        {
            ExternalId = source.Id,
            ExternalAccountId = source.AccountId,
            AccountId = domainAccountId,
            TransactionDateTime = source.Date,
            Description = source.Description,
            Amount = source.Amount,
            Category = ToDomainTransactionCategory(source.Category, source.Type),
            Kind = ToDomainTransactionKind(source.Type, warnings),
            RecognitionData = ToRecognitionData(source),
        };

        transaction.SetPayloadJson(source);

        return warnings.Count == 0
            ? new MappingResult<Transaction>.Complete(transaction)
            : new MappingResult<Transaction>.Partial(transaction, warnings);

    }

    private static TransactionRecognitionData ToRecognitionData(AkahuTransaction source)
    {
        return new TransactionRecognitionData
        {
            // Meta
            Code = source.Meta?.Code,
            Reference = source.Meta?.Reference,
            CardSuffix = source.Meta?.CardSuffix,
            Particulars = source.Meta?.Particulars,
            CounterpartyAccount = source.Meta?.OtherAccount,

            // Category
            Category = source.Category?.Name,
            Group = source.Category?.Groups?
                .GetValueOrDefault(AkahuDefaultTransactionsGroupKey)?
                .Name,

            // Merchant
            MerchantName = source.Merchant?.Name,
        };
    }

    private static TransactionKind ToDomainTransactionKind(
        AkahuTransactionType source,
        Dictionary<string, List<string>> warnings
        )
    {
        return source switch
        {
            AkahuTransactionType.Credit => TransactionKind.GenericCredit,
            AkahuTransactionType.Debit => TransactionKind.GenericDebit,
            AkahuTransactionType.Payment => TransactionKind.ExternalPayment,
            AkahuTransactionType.Transfer => TransactionKind.Transfer,
            AkahuTransactionType.StandingOrder => TransactionKind.StandingOrder,
            AkahuTransactionType.Eftpos => TransactionKind.CardPayment,
            AkahuTransactionType.Interest => TransactionKind.Interest,
            AkahuTransactionType.Fee => TransactionKind.Fee,
            AkahuTransactionType.Tax => TransactionKind.Tax,
            AkahuTransactionType.CreditCard => TransactionKind.CreditCardPayment,
            AkahuTransactionType.DirectCredit => TransactionKind.DirectCredit,
            AkahuTransactionType.DirectDebit => TransactionKind.DirectDebit,
            AkahuTransactionType.Atm => TransactionKind.CashWithdrawalOrDeposit,
            AkahuTransactionType.Loan => TransactionKind.LoanPayment,
            _ => RegisterWarning(
                warnings,
                "type",
                $"Unexpected Akahu transaction type value: {source}",
                TransactionKind.Unspecified),
        };
    }

    private static TransactionCategory ToDomainTransactionCategory(
        AkahuTransactionCategory? sourceCategory,
        AkahuTransactionType type)
    {
        var categoryFromType = type switch
        {
            AkahuTransactionType.Interest => TransactionCategory.InterestIncome,
            AkahuTransactionType.Tax => TransactionCategory.Taxes,
            _ => (TransactionCategory?)null,
        };

        if (categoryFromType is not null)
        {
            return categoryFromType.Value;
        }

        var groupName = sourceCategory?.Groups?
            .GetValueOrDefault(AkahuDefaultTransactionsGroupKey)?
            .Name;

        if (groupName is null) return TransactionCategory.Default;

        TransactionCategory? category = groupName.Trim().ToUpperInvariant() switch
        {
            "EDUCATION" => TransactionCategory.Education,
            "FOOD" => TransactionCategory.Food,
            "HEALTH" => TransactionCategory.Health,
            "HOUSEHOLD" => TransactionCategory.Household,
            "HOUSING" => TransactionCategory.Housing,
            "LIFESTYLE" => TransactionCategory.Lifestyle,
            "PROFESSIONAL SERVICES" => TransactionCategory.ProfessionalServices,
            "TRANSPORT" => TransactionCategory.Transport,
            "UTILITIES" => TransactionCategory.Utilities,
            _ => null,
        };

        return category is not null ? category.Value : TransactionCategory.Default;

    }

    private static T RegisterWarning<T>(
        Dictionary<string, List<string>> register,
        string field,
        string message,
        T result)
    {
        register.TryAdd(field, []);
        register[field].Add(message);

        return result;
    }
}
