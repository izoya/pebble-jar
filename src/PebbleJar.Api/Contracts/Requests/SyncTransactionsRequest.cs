namespace PebbleJar.Api.Contracts.Requests
{
    public sealed class SyncTransactionsRequest
    {
        public Guid? AccountId { get; init; }
        public DateTimeOffset? FromDate { get; init; }
    }
}
