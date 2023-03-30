using iRLeagueApiCore.Client.Endpoints;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using Microsoft.AspNetCore.Identity.Test;
using Newtonsoft.Json;
using System.Net;

namespace iRLeagueApiCore.UnitTests.Client.Endpoints;

public sealed class EndpointsTests
{
    public static string BaseUrl = "https://example.com/api/";

    [Fact]
    public void Endpoint_AddQueryParameter_ShouldAddQueryParameters()
    {
        var name = "param1";
        var value = "value1";
        var routeBuilder = new RouteBuilder();
        routeBuilder.AddEndpoint(BaseUrl);
        var endpoint = new TestEndpoint(new(new(), Mock.Of<IAsyncTokenProvider>()), routeBuilder);

        endpoint.AddQueryParameter(x => x.Add(name, value));
        var route = endpoint.RouteBuilder.Build();

        route.Should().Be($"{BaseUrl}?{name}={value}");
    }

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
        HttpMethod? requestMethod = default;
        var httpMessageHandler = MockHelpers.TestMessageHandler(x =>
        {
            requestUrl = x.RequestUri?.AbsoluteUri ?? string.Empty;
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

    private class TestEndpoint : EndpointBase
    {
        public new RouteBuilder RouteBuilder { get => base.RouteBuilder; }

        public TestEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) : base(httpClient, routeBuilder)
        {
        }
    }
}
