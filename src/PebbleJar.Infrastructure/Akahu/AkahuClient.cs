using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PebbleJar.Infrastructure.Akahu.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PebbleJar.Infrastructure.Akahu
{
    public sealed class AkahuClient(
        HttpClient httpClient,
        IOptions<AkahuOptions> options,
        ILogger<AkahuClient> logger)
    {
        private readonly AkahuOptions options = options.Value;

        public async Task<AkahuSingleResponse<AkahuUser>> GetMeAsync(
            CancellationToken token)
        {
            string endpoint = "me";
            using var request = PrepareRequestFor("me");
            using var response = await httpClient.SendAsync(request, token);

            response.EnsureSuccessStatusCode();

            return await IntoAkahuResponse<AkahuSingleResponse<AkahuUser>>(
                endpoint, response, token);
        }

        public async Task<AkahuListResponse<AkahuAccount>> ListAccountsAsync(CancellationToken token)
        {
            string endpoint = "accounts";
            using var request = PrepareRequestFor(endpoint);
            using var response = await httpClient.SendAsync(request, token);

            response.EnsureSuccessStatusCode();

            return await IntoAkahuResponse<AkahuListResponse<AkahuAccount>>(
                endpoint, response, token);
        }


        private async Task<TResponse> IntoAkahuResponse<TResponse>(
            string endpoint,
            HttpResponseMessage response,
            CancellationToken token)
        where TResponse : AkahuResponseBase
        {
            var obj = await response.Content
                .ReadFromJsonAsync<TResponse>(token)
                ?? throw new InvalidOperationException("Akahu returned an empty response.");

            LogUnknownFields<TResponse>(obj, endpoint);

            return obj;
        }

        private HttpRequestMessage PrepareRequestFor(string endpoint, HttpMethod? method = null)
        {
            var requestMethod = method ?? HttpMethod.Get;

            var request = new HttpRequestMessage(requestMethod, endpoint);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                options.UserAccessToken.Reveal());

            request.Headers.Add(
                "X-Akahu-Id",
                options.AppIdToken.Reveal());

            return request;

        }

        private void LogUnknownFields<TResponse>(
            TResponse obj,
            string endpoint
        ) where TResponse : AkahuResponseBase
        {
            var fieldsCollection = new Dictionary<string, string[]>()
            {
                ["outer"] = [.. obj.UnknownFields.Keys],
                ["inner"] = [.. obj.ListItems()
                .SelectMany(item => item.UnknownFields.Keys)
                // Ordinal (explicitly stated default) performs a case-sensitive comparison
                .Distinct(StringComparer.Ordinal)],
            };

            foreach ((var key, var fields) in fieldsCollection)
            {
                if (fields.Length > 0)
                {
                    logger.LogWarning(
                        "Akahu returned unmapped fields in {ResponseLevel} Response " +
                        "for /{Endpoint}: {FieldNames}",
                        key,
                        endpoint,
                        fields
                    );
                }
            }
        }
    }
}

