using iRLeagueApiCore.Client.Endpoints.Bonuses;
using iRLeagueApiCore.Client.Endpoints.Penalties;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Results;
internal sealed class SessionResultByIdEndpoint : EndpointBase, ISessionResultByIdEndpoint
{
    public SessionResultByIdEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long id) 
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddParameter(id);
    }

    public IGetAllEndpoint<PenaltyModel> Penalties()
    {
        return new PenaltiesEndpoint(HttpClientWrapper, RouteBuilder);
    }

    public IGetAllEndpoint<AddBonusModel> Bonuses()
    {
        return new BonusesEndpoint(HttpClientWrapper, RouteBuilder);
    }

    public IWithIdEndpoint<IResultRowByIdEndpoint> Rows()
    {
        return new ResultRowsEndpoint(HttpClientWrapper, RouteBuilder);
    }
}
