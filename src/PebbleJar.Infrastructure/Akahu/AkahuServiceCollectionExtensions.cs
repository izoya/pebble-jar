using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PebbleJar.Application.Interfaces;

namespace PebbleJar.Infrastructure.Akahu;

/// <summary>
/// - Configures Akahu options
/// - Registers a named or typed HttpClient
/// - Registers the account sync service
/// </summary>
public static class AkahuServiceCollectionExtensions
{
    public static IServiceCollection AddAkahuService(
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

        services.AddScoped<IAccountSyncService, AccountSyncService>();

        return services;
    }
}
