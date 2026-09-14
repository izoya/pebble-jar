using PebbleJar.Application.Queries;
using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Api.Contracts.Requests;

public class TransactionsSearchGroupRequest()
    : TransactionSearchBaseRequest(defaultPageSize: 20)
{
    [Required]
    [EnumDataType(typeof(TransactionGrouping))]
    public TransactionGrouping? Grouping { get; init; }
}
