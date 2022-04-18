using iRLeagueApiCore.Communication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Results
{
    public static class HttpExtensions
    {
        public static async Task<ClientActionResult<T>> ToClientActionResultAsync<T>(this HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
        {
            try
            {
                if (httpResponse.IsSuccessStatusCode)
                {
                    var content = await httpResponse.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
                    return new ClientActionResult<T>(content);
                }

                string message;
                IEnumerable<object> errors;
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        {
                            var response = await httpResponse.Content.ReadFromJsonAsync<BadRequestResponse>(cancellationToken: cancellationToken);
                            message = response.Status;
                            errors = response.Errors.Cast<object>();
                            break;
                        }
                    case HttpStatusCode.Forbidden:
                        {
                            message = "Forbidden";
                            errors = new object[0];
                            break;
                        }
                    case HttpStatusCode.InternalServerError:
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                            message = response;
                            errors = new object[] { "Internal server Error " };
                            break;
                        }
                    default:
                        message = "Unknown Response";
                        errors = new object[0];
                        break;
                }
                return new ClientActionResult<T>(false, message, default, httpResponse.StatusCode, errors);
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                var errors = new object[] { ex };
                return new ClientActionResult<T>(false, ex.Message, default, 0, errors);
            }
        }

        public static async Task<ClientActionResult<T>> AsClientActionResultAsync<T>(this Task<HttpResponseMessage> request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await (await request).ToClientActionResultAsync<T>(cancellationToken);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException)
            {
                var errors = new object[] { ex };
                return new ClientActionResult<T>(false, ex.Message, default, 0, errors);
            }
        }

        public static async Task<ClientActionResult<T>> GetAsClientActionResult<T>(this HttpClient httpClient, string query, CancellationToken cancellationToken = default)
        {
            return await (await httpClient.GetAsync(query, cancellationToken)).ToClientActionResultAsync<T>(cancellationToken);
        }

        public static async Task<ClientActionResult<TResponse>> PostAsClientActionResult<TResponse, TPost>(this HttpClient httpClient, string query, TPost body, CancellationToken cancellationToken = default)
        {
            return await (await httpClient.PostAsJsonAsync(query, body, cancellationToken)).ToClientActionResultAsync<TResponse>(cancellationToken);
        }

        public static async Task<ClientActionResult<TResponse>> PutAsClientActionResult<TResponse, TPut>(this HttpClient httpClient, string query, TPut body , CancellationToken cancellationToken = default)
        {
            return await (await httpClient.PutAsJsonAsync(query, body, cancellationToken)).ToClientActionResultAsync<TResponse>(cancellationToken);
        }

        public static async Task<ClientActionResult<NoContent>> DeleteAsClientActionResult(this HttpClient httpClient, string query, CancellationToken cancellationToken= default)
        {
            return await (await httpClient.DeleteAsync(query, cancellationToken)).ToClientActionResultAsync<NoContent>(cancellationToken);
        }
    }
}
