using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Protests;

public sealed class ProtestEndpoint : PostGetAllEndpoint<ProtestModel, PostProtestModel>
{
    public ProtestEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Protests");
    }
}
