using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Client.Endpoints.Leagues
{
    public class LeaguesEndpointTests
    {
        private const string BaseUrl = "https://test.com/api/";

        [Fact]
        public async Task ShouldCallCorrectRequestUrlGetAllLeagues()
        {
            const string shouldRequestUrl = BaseUrl + "Leagues";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var leagues = new List<GetLeagueModel>()
            {
                new GetLeagueModel() { Id = 1 },
                new GetLeagueModel() { Id = 2 },
            };
            var content = new StringContent(JsonConvert.SerializeObject(leagues));
            string requestUrl = "";
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage message, CancellationToken cancellationToken) =>
                {
                    requestUrl = message.RequestUri.AbsoluteUri;
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = content
                    };
                });

            var testClient = new HttpClient(mockHttpMessageHandler.Object);
            testClient.BaseAddress = new Uri(BaseUrl);
            ILeaguesEndpoint endpoint = new LeaguesEndpoint(testClient, new RouteBuilder());
            await endpoint.Get();

            Assert.Equal(shouldRequestUrl, requestUrl);
        }
    }
}
