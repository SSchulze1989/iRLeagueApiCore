using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Standings;

internal sealed class StandingResultRowByIdEndpoint : EndpointBase, IStandingResultRowByIdEndpoint
{
    public StandingResultRowByIdEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long scoredResultRowId) : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddParameter(scoredResultRowId);
    }

    public IDropweekOverrideEndpoint DropweekOverride()
    {
        return new DropweekOverrideEndpoint(HttpClientWrapper, RouteBuilder);
    }
}
