namespace PebbleJar.Application.Interfaces;

public interface IAccountSyncService
{
    Task RefreshAsync(CancellationToken token);
}
