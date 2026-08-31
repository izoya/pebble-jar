using PebbleJar.Extensions.Types;
using System.Text.Json;
using Xunit;

namespace PebbleJar.Extensions.Tests.Types;

public sealed class SecretStringTests
{
    private const string TestSecret = "test-token-must-never-be-exposed";

    [Fact]
    public void ToString_RedactsTheSecret()
    {
        var secret = new SecretString(TestSecret);

        var dump = secret.ToString();

        Assert.Equal("[REDACTED]", dump);
        Assert.DoesNotContain(TestSecret, dump);
    }

    [Fact]
    public void Serialize_RedactsTheSecret()
    {
        var secret = new SecretString(TestSecret);

        var json = JsonSerializer.Serialize(secret);

        Assert.Equal("\"[REDACTED]\"", json);
        Assert.DoesNotContain(TestSecret, json);
    }

    [Fact]
    public void Reveal_RevealsTheSecret()
    {
        var secret = new SecretString(TestSecret);

        Assert.Equal("[REDACTED]", $"{secret}");
        Assert.Equal(TestSecret, secret.Reveal());
    }
}
