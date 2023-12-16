using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Bonuses;
internal sealed class BonusesEndpoint : PostGetAllEndpoint<AddBonusModel, PostAddBonusModel>, IBonusesEndpoint, IPostGetAllEndpoint<AddBonusModel, PostAddBonusModel>
{
    public BonusesEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) 
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Bonuses");
    }

    public IUpdateEndpoint<AddBonusModel, PutAddBonusModel> WithId(long id)
    {
        return new BonusByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
