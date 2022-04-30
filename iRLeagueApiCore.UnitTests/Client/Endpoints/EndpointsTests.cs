using iRLeagueApiCore.Client.Http;
using Microsoft.AspNetCore.Identity.Test;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Client.Endpoints
{
    public class EndpointsTests
    {
        public static string BaseUrl = "https://example.com/api/";

        public static async Task TestRequestUrl<TEndpoint>(string expectedUrl, Func<HttpClientWrapper, TEndpoint> endpoint, Func<TEndpoint, Task> action)
        {
            var content = new StringContent(JsonConvert.SerializeObject(null));
            string requestUrl = "";
            var httpMessageHandler = MockHelpers.TestMessageHandler(x =>
            {
                requestUrl = x.RequestUri.AbsoluteUri;
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content,
                };
            });

            var testClient = new HttpClient(httpMessageHandler);
            testClient.BaseAddress = new Uri(BaseUrl);
            var testClientWrapper = new HttpClientWrapper(testClient, Mock.Of<IAsyncTokenProvider>());
            await action.Invoke(endpoint.Invoke(testClientWrapper));

            Assert.Equal(expectedUrl, requestUrl);
        }
    }
}
