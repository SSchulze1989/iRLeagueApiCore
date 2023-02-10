using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Championships;
public class ChampSeasonByIdEndpoint : UpdateEndpoint<ChampSeasonModel, PutChampSeasonModel>, IChampSeasonByIdEndpoint
{
    public ChampSeasonByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long id) : 
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(id);
    }
}
