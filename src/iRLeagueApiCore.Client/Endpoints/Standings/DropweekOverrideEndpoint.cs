using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Standings;

internal sealed class DropweekOverrideEndpoint : UpdateEndpoint<DropweekOverrideModel, PutDropweekOverrideModel>, IDropweekOverrideEndpoint
{
    public DropweekOverrideEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Dropweek");
    }
}
