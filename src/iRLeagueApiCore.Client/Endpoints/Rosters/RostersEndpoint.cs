using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;
internal sealed class RostersEndpoint : PostGetAllEndpoint<RosterInfoModel, RosterModel, PostRosterModel>, IRostersEndpoint
{
    public RostersEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Rosters");
    }

    IRosterByIdEndpoint IWithIdEndpoint<IRosterByIdEndpoint, long>.WithId(long id)
    {
        return new RosterByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
