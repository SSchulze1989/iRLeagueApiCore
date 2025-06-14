using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;

internal sealed class RosterEntryByIdEndpoint : PutEndpoint<RosterEntryModel, RosterEntryModel>, IRosterEntryByIdEndpoint 
{
    public RosterEntryByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long entryId)
        : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(entryId);
    }

    public async Task<ClientActionResult<NoContent>> Delete(CancellationToken cancellationToken = default)
    {
        return await HttpClientWrapper.DeleteAsClientActionResult(QueryUrl, cancellationToken);
    }
}
