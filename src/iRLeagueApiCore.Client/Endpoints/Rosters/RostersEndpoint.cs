using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models.Rosters;
using System.Reflection;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;
internal sealed class RostersEndpoint : PostGetAllEndpoint<RosterInfoModel, RosterModel, PostRosterModel>, IRostersEndpoint, IGetAllEndpoint<RosterModel>
{
    public RostersEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Rosters");
    }

    async Task<ClientActionResult<IEnumerable<RosterModel>>> IGetEndpoint<IEnumerable<RosterModel>>.Get(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.GetAsClientActionResult<IEnumerable<RosterModel>>(QueryUrl, cancellationToken);
    }

    IRosterByIdEndpoint IWithIdEndpoint<IRosterByIdEndpoint, long>.WithId(long id)
    {
        return new RosterByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
