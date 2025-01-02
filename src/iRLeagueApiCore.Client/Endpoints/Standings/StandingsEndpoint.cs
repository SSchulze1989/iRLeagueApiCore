using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Standings;

namespace iRLeagueApiCore.Client.Endpoints.Standings;

internal sealed class StandingsEndpoint : GetAllEndpoint<StandingsModel>, IStandingsEndpoint, IEventStandingsEndpoint
{
    public StandingsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Standings");
    }

    public IPostEndpoint<bool> Calculate()
    {
        return new CalculateEndpoint(HttpClientWrapper, RouteBuilder);
    }

    public IStandingByIdEndpoint WithId(long id)
    {
        return new StandingByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
