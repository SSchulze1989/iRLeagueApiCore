using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results;

internal sealed class RawResultsEndpoint : UpdateEndpoint<RawEventResultModel, RawEventResultModel>
{
    public RawResultsEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Raw");
    }
}
