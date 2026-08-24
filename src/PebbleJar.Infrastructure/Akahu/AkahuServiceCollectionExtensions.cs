using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PebbleJar.Infrastructure.Akahu;

/// <summary>
/// - Configures Akahu options
/// - Registers a named or typed HttpClient
/// - (NOT YET) Registers AkahuClient as the implementation of the Application interface
/// </summary>
public static class AkahuServiceCollectionExtensions
{
    public static IServiceCollection AddAkahu(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AkahuOptions>()
            .Bind(configuration.GetSection("Akahu"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<AkahuClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AkahuOptions>>().Value;
            client.BaseAddress = options.BaseUrl;
        });

        return services;
    }
}
