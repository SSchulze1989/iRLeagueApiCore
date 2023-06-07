using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints;

internal class EndpointBase : IEndpoint
{
    protected HttpClientWrapper HttpClientWrapper { get; }
    protected RouteBuilder RouteBuilder { get; }

    public virtual string QueryUrl => RouteBuilder.Build();

    public EndpointBase(HttpClientWrapper httpClient, RouteBuilder routeBuilder)
    {
        HttpClientWrapper = httpClient;
        RouteBuilder = routeBuilder.Copy();
    }

    void IEndpoint.WithParameters(Func<IParameterBuilder, IParameterBuilder> parameterBuilder)
    {
        RouteBuilder.WithParameters(parameterBuilder);
    }
}
