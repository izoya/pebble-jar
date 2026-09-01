using System.Net;

namespace PebbleJar.Infrastructure.Akahu;

public sealed class AkahuRequestException(
    string? endpoint,
    HttpStatusCode statusCode,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string Endpoint { get; } = endpoint;
    public HttpStatusCode StatusCode { get; } = statusCode;
}
