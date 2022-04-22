using Microsoft.AspNetCore.Identity.Test;
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

        public static async Task TestRequestUrl<TEndpoint>(string expectedUrl, Func<HttpClient, TEndpoint> endpoint, Func<TEndpoint, Task> action)
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
            await action.Invoke(endpoint.Invoke(testClient));

            Assert.Equal(expectedUrl, requestUrl);
        }
    }
}
