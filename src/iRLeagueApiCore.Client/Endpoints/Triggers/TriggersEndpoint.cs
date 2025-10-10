using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Triggers;
internal sealed class TriggersEndpoint : PostGetAllEndpoint<TriggerModel, PostTriggerModel>, ITriggersEndpoint
{
    public TriggersEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Triggers");
    }

    public ITriggerByIdEndpoint WithId(long id)
    {
        return new TriggerByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
