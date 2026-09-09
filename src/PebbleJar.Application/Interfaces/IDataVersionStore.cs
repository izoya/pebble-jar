using PebbleJar.Domain;

namespace PebbleJar.Application.Interfaces;

public interface IDataVersionStore
{
    Task IncrementAsync(DataScope scope);

    Task<int> GetAsync(DataScope scope);
}
