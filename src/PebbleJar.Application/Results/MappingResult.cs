namespace PebbleJar.Application.Results;

public abstract record MappingResult<T>
{
    public sealed record Complete(T Value)
        : MappingResult<T>;

    public sealed record Partial(
        T Value,
        Dictionary<string, List<string>> Warnings)
        : MappingResult<T>;

    public sealed record Failed(
        Dictionary<string, string> Errors)
        : MappingResult<T>;
}
