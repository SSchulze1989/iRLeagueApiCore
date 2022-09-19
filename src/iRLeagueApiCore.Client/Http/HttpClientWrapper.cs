using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Http
{
    public class HttpClientWrapper
    {
        private readonly HttpClient httpClient;
        private readonly IAsyncTokenProvider tokenProvider;
        private readonly ILeagueApiClient apiClient;
        private readonly JsonSerializerOptions jsonOptions;

        public HttpClientWrapper(HttpClient httpClient, IAsyncTokenProvider tokenProvider, ILeagueApiClient apiClient = default, JsonSerializerOptions jsonOptions = default)
        {
            this.httpClient = httpClient;
            this.tokenProvider = tokenProvider;
            this.apiClient = apiClient;
            this.jsonOptions = jsonOptions;
        }

        public async Task<HttpResponseMessage> Get(string uri, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri, UriKind.RelativeOrAbsolute));
            return await SendRequest(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> Post(string uri, object data, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, UriKind.RelativeOrAbsolute));
            request.Content = new StringContent(JsonSerializer.Serialize(data, jsonOptions), Encoding.UTF8, "application/json");
            return await SendRequest(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> Put(string uri, object data, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri, UriKind.RelativeOrAbsolute));
            request.Content = new StringContent(JsonSerializer.Serialize(data, jsonOptions), Encoding.UTF8, "application/json");
            return await SendRequest(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> Delete(string uri, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(uri, UriKind.RelativeOrAbsolute));
            return await SendRequest(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddJWTTokenAsync(request);
            var result = await httpClient.SendAsync(request, cancellationToken);
            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await apiClient?.LogOut();
            }
            return result;
        }

        private async Task AddJWTTokenAsync(HttpRequestMessage request)
        {
            var token = await tokenProvider.GetTokenAsync();

            if (string.IsNullOrEmpty(token) == false)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            }
        }

        public async Task<ClientActionResult<T>> GetAsClientActionResult<T>(string query, CancellationToken cancellationToken = default)
        {
            return await Get(query, cancellationToken).AsClientActionResultAsync<T>(jsonOptions, cancellationToken);
        }

        public async Task<ClientActionResult<T>> PostAsClientActionResult<T>(string query, object body, CancellationToken cancellationToken = default)
        {
            return await Post(query, body, cancellationToken).AsClientActionResultAsync<T>(jsonOptions, cancellationToken);
        }

        public async Task<ClientActionResult<T>> PutAsClientActionResult<T>(string query, object body, CancellationToken cancellationToken = default)
        {
            return await Put(query, body, cancellationToken).AsClientActionResultAsync<T>(jsonOptions, cancellationToken);
        }

        public async Task<ClientActionResult<NoContent>> DeleteAsClientActionResult(string query, CancellationToken cancellationToken = default)
        {
            return await Delete(query, cancellationToken).AsClientActionResultAsync<NoContent>(jsonOptions, cancellationToken);
        }
    }
}
