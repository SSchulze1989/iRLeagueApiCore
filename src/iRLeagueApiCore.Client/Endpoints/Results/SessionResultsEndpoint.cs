using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Results;
internal sealed class SessionResultsEndpoint : EndpointBase, ISessionResultsEndpoint
{
    public SessionResultsEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) 
        : base(httpClient, routeBuilder, "ScoredSessionResults")
    {
    }

    public ISessionResultByIdEndpoint WithId(long id)
    {
        return new SessionResultByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
