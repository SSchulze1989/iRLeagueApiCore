using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Standings;
internal sealed class StandingResultRowsEndpoint : EndpointBase, IWithIdEndpoint<IStandingResultRowByIdEndpoint>
{
    public StandingResultRowsEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder)
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddEndpoint("ResultRows");
    }

    public IStandingResultRowByIdEndpoint WithId(long id)
    {
        return new StandingResultRowByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
