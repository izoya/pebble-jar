using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Api.Contracts.Requests
{
    public class ListTransactionsRequest
    {
        public Guid? AccountId { get; init; }
        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }

        [Range(1, int.MaxValue)]
        public int? PageNumber { get; init; }

        [Range(1, 100)]
        public int? PageSize { get; init; }
    }
}
