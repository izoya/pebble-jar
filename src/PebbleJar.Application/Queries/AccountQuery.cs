namespace PebbleJar.Application.Queries;

public sealed record AccountQuery(
    Guid[]? Ids = null,
    bool? IsSyncEnabled = null,
    bool? WithProvider = null);
