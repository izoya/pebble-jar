using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PebbleJar.Infrastructure.Akahu.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PebbleJar.Infrastructure.Akahu
{
    public sealed class AkahuClient
    {
        private readonly HttpClient _httpClient;
        private readonly AkahuOptions _options;
        private readonly ILogger<AkahuClient> logger;

        public AkahuClient(
            HttpClient httpClient,
            IOptions<AkahuOptions> options,
            ILogger<AkahuClient> logger
            )
        {
            _httpClient = httpClient;
            _options = options.Value;
            this.logger = logger;
        }


        public async Task<AkahuResponse<AkahuUser>> GetMeAsync(
            CancellationToken token)
        {
            using var request = PrepareRequestFor("me");
            using var response = await _httpClient.SendAsync(request, token);

            response.EnsureSuccessStatusCode();

            return await IntoAkahuResponse<AkahuUser>(response, token);
        }

        private async Task<AkahuResponse<T>> IntoAkahuResponse<T>(HttpResponseMessage response, CancellationToken token)
            where T : AkahuDto
        {
            var obj = await response.Content
                .ReadFromJsonAsync<AkahuResponse<T>>(token)
                ?? throw new InvalidOperationException("Akahu returned an empty response.");

            LogUnknownFields(obj);

            return obj;
        }

        private HttpRequestMessage PrepareRequestFor(string endpoint, HttpMethod? method = null)
        {
            var requestMethod = method ?? HttpMethod.Get;

            var request = new HttpRequestMessage(requestMethod, endpoint);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.UserAccessToken.Reveal());

            request.Headers.Add(
                "X-Akahu-Id",
                _options.AppIdToken.Reveal());

            return request;

        }

        private void LogUnknownFields<T>(AkahuResponse<T> obj)
            where T : AkahuDto
        {
            var fieldsCollection = new Dictionary<string, string[]>() {

                { "outer", obj.UnknownFields.Keys.ToArray() },
                { "inner", obj.Item.UnknownFields.Keys.ToArray() },
            };

            foreach ((var key, var fields) in fieldsCollection)
            {
                if (fields.Length > 0)
                {
                    logger.LogWarning(
                        "Akahu returned unmapped fields in {ResponseLevel} Response: {FieldNames}",
                        key,
                        fields
                    );
                }
            }
        }
    }
}


