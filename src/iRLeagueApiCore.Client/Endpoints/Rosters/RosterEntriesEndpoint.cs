using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;

internal sealed class RosterEntriesEndpoint : EndpointBase, IWithIdEndpoint<IRosterEntryByIdEndpoint>
{
    public RosterEntriesEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder)
        : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Entries");
    }

    public IRosterEntryByIdEndpoint WithId(long entryId)
    {
        return new RosterEntryByIdEndpoint(HttpClientWrapper, RouteBuilder, entryId);
    }
}
