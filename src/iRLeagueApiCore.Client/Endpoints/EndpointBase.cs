using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints;

public class EndpointBase
{
    protected HttpClientWrapper HttpClientWrapper { get; }
    protected RouteBuilder RouteBuilder { get; }

    protected virtual string QueryUrl => RouteBuilder.Build();

    public EndpointBase(HttpClientWrapper httpClient, RouteBuilder routeBuilder)
    {
        HttpClientWrapper = httpClient;
        RouteBuilder = routeBuilder.Copy();
    }
}
