using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results;

internal class PointSystemsEndpoint : PostGetAllEndpoint<PointSystemModel, PostPointSystemModel>, IPointSystemsEndpoint
{
    public PointSystemsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("PointSystems");
    }

    public IPointSystemByIdEndpoint WithId(long id)
    {
        return new PointSystemByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
