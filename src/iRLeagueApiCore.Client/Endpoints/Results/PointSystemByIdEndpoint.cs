using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results;

internal class PointSystemByIdEndpoint : UpdateEndpoint<PointSystemModel, PutPointSystemModel>, IPointSystemByIdEndpoint
{
    public PointSystemByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long resultConfigId) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(resultConfigId);
    }
}
