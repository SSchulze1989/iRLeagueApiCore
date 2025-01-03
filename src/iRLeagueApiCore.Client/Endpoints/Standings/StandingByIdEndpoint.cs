using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Standings;

internal sealed class StandingByIdEndpoint : EndpointBase, IStandingByIdEndpoint
{
    public StandingByIdEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long standingId) : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddParameter(standingId);
    }

    public IWithIdEndpoint<IStandingResultRowByIdEndpoint> ResultRows()
    {
        return new StandingResultRowsEndpoint(HttpClientWrapper, RouteBuilder);
    }
}
