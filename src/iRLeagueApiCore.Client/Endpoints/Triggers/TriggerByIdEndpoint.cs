using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Triggers;

internal sealed class TriggerByIdEndpoint : UpdateEndpoint<TriggerModel, PutTriggerModel>, ITriggerByIdEndpoint
{
    public TriggerByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long triggerId) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(triggerId);
    }

    public IPostEndpoint<NoContent> RunTrigger()
    {
        return new CustomEndpoint<NoContent>(HttpClientWrapper, RouteBuilder, "Run");
    }
}
