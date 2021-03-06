using iRLeagueApiCore.Client;
using iRLeagueApiCore.Client.Http;
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
        public async Task ShouldSetAuthenticationToken()
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

            string token = null;
            var mockTokenStore = new Mock<ITokenStore>();
            mockTokenStore.Setup(x => x.SetTokenAsync(It.IsAny<string>()))
                .Callback<string>(x => token = x);

            var httpClient = new HttpClient(messageHandler);
            httpClient.BaseAddress = new Uri(baseUrl);

            var apiClient = new LeagueApiClient(Logger, httpClient, mockTokenStore.Object);

            var result = await apiClient.LogIn("testUser", "testPassword");

            Assert.True(result);
            Assert.Equal(testToken, token);
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
            var mockTokenStore = new Mock<ITokenStore>();
            mockTokenStore.Setup(x => x.GetTokenAsync())
                .ReturnsAsync(testToken);
            var httpClient = new HttpClient(messageHandler);
            httpClient.BaseAddress = new Uri(baseUrl);

            var apiClient = new LeagueApiClient(Logger, httpClient, mockTokenStore.Object);
            await apiClient.Leagues().Get();

            Assert.Equal("bearer", authHeader?.Scheme, ignoreCase: true);
            Assert.Equal(testToken, authHeader?.Parameter);
        }

        [Fact]
        public async Task ShouldNotSendAuthenticatedRequestAfterLogOut()
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

            string token = testToken;
            var mockTokenStore = new Mock<ITokenStore>();
            mockTokenStore.Setup(x => x.SetTokenAsync(It.IsAny<string>()))
                .Callback<string>(x => token = x);
            mockTokenStore.Setup(x => x.ClearTokenAsync())
                .Callback(() => token = null);
            mockTokenStore.Setup(x => x.GetTokenAsync())
                .ReturnsAsync(testToken);
            var httpClient = new HttpClient(messageHandler);
            httpClient.BaseAddress = new Uri(baseUrl);

            var apiClient = new LeagueApiClient(Logger, httpClient, mockTokenStore.Object);
            await apiClient.LogOut();
            await apiClient.Leagues().Get();

            Assert.True(string.IsNullOrEmpty(token));
        }
    }
}
