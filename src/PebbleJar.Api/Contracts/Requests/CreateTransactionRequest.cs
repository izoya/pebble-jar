using PebbleJar.Domain;
using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Api.Contracts.Requests;

public class CreateTransactionRequest
{
    [Required]
    public Guid? AccountId { get; init; }
    [Required]
    public decimal? Amount { get; init; }
    [Required]
    [EnumDataType(typeof(TransactionCategory))]
    public TransactionCategory? Category { get; init; }
    [StringLength(200)]
    public string? Description { get; init; }
}
