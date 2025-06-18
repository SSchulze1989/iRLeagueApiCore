using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;

internal sealed class RosterByIdEndpoint : UpdateEndpoint<RosterModel, PutRosterModel>, IRosterByIdEndpoint
{
    public RosterByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long rosterId) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(rosterId);
    }

    public IWithIdEndpoint<IRosterEntryByIdEndpoint> Entries()
    {
        return new RosterEntriesEndpoint(HttpClientWrapper, RouteBuilder);
    }
}
