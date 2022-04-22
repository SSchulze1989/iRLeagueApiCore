using iRLeagueApiCore.Client;
using iRLeagueApiCore.UnitTests.Client.Endpoints;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Client
{
    public class LeagueApiClientTests
    {
        private const string baseUrl = "https://example.com/api";
        private const string testToken = "aslkgjwuipoht2io3ro2pqhuishgiag";

        private ILogger<LeagueApiClient> Logger { get; } = new Mock<ILogger<LeagueApiClient>>().Object;

        [Fact]
        public async Task ShouldSetAuthenticationHeader()
        {
            var messageHandler = MockHelpers.TestMessageHandler(x => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    token = testToken,
                    expiration = DateTime.UtcNow.AddDays(1),
                })),
            });

            var httpClient = new HttpClient(messageHandler);
            httpClient.BaseAddress = new Uri(baseUrl);

            var apiClient = new LeagueApiClient(Logger, httpClient);

            var result = await apiClient.LogIn("testUser", "testPassword");

            Assert.True(result);
            var authHeader = httpClient.DefaultRequestHeaders.Authorization;
            Assert.Equal("bearer", authHeader.Scheme, ignoreCase: true);
            Assert.Equal(testToken, authHeader.Parameter);
        }

        [Fact]
        public async Task ShouldSendAuthenticatedRequest()
        {
            AuthenticationHeaderValue authHeader = default;
            var messageHandler = MockHelpers.TestMessageHandler(x =>
            {
                authHeader = x.Headers.Authorization;
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(default)),
                };
            });

            var httpClient = new HttpClient(messageHandler);
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testToken);

            var apiClient = new LeagueApiClient(Logger, httpClient);
            await apiClient.Leagues().Get();

            Assert.Equal("bearer", authHeader?.Scheme, ignoreCase: true);
            Assert.Equal(testToken, authHeader?.Parameter);
        }
    }
}
