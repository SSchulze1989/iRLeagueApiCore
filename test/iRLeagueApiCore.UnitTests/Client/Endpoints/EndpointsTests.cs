using iRLeagueApiCore.Client.Http;
using Microsoft.AspNetCore.Identity.Test;
using Newtonsoft.Json;
using System.Net;

namespace iRLeagueApiCore.UnitTests.Client.Endpoints;

public class EndpointsTests
{
    public static string BaseUrl = "https://example.com/api/";

    public static async Task TestRequestUrl<TEndpoint>(string expectedUrl, Func<HttpClientWrapper, TEndpoint> endpoint, Func<TEndpoint, Task> action)
    {
        var content = new StringContent(JsonConvert.SerializeObject(null));
        string requestUrl = "";
        var httpMessageHandler = MockHelpers.TestMessageHandler(x =>
        {
            requestUrl = x.RequestUri!.AbsoluteUri;
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = content,
            };
        });

        var testClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri(BaseUrl)
        };
        var testClientWrapper = new HttpClientWrapper(testClient, Mock.Of<IAsyncTokenProvider>());
        await action.Invoke(endpoint.Invoke(testClientWrapper));

        requestUrl.Should().Be(expectedUrl);
    }

    public static async Task TestRequest<TEndpoint>(string expectedUrl, Func<HttpClientWrapper, TEndpoint> endpoint, Func<TEndpoint, Task> action, HttpMethod method)
    {
        var content = new StringContent(JsonConvert.SerializeObject(null));
        string requestUrl = "";
        HttpMethod requestMethod = default;
        var httpMessageHandler = MockHelpers.TestMessageHandler(x =>
        {
            requestUrl = x.RequestUri.AbsoluteUri;
            requestMethod = x.Method;
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = content,
            };
        });

        var testClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri(BaseUrl)
        };
        var testClientWrapper = new HttpClientWrapper(testClient, Mock.Of<IAsyncTokenProvider>());
        await action.Invoke(endpoint.Invoke(testClientWrapper));

        requestUrl.Should().Be(expectedUrl);
        requestMethod.Should().Be(method);
    }
}
