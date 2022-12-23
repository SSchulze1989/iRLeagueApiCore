using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Leagues;

public class LeagueByIdEndpoint : UpdateEndpoint<LeagueModel, PutLeagueModel>, ILeagueByIdEndpoint
{
    public LeagueByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long leagueId) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(leagueId);
    }
}
