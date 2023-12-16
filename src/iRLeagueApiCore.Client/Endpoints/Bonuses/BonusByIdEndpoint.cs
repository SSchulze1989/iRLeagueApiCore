using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Bonuses;
internal sealed class BonusByIdEndpoint : UpdateEndpoint<AddBonusModel, PutAddBonusModel>
{
    public BonusByIdEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long id) 
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddParameter(id);
    }
}
