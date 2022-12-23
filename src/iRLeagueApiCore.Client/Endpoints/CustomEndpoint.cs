using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints;

public class CustomEndpoint : UpdateEndpoint<object, object>, ICustomEndpoint
{
    private string Route { get; }

    protected override string QueryUrl => Route;

    public CustomEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, string route) : base(httpClientWrapper, routeBuilder)
    {
        Route = route;
    }

    async Task<ClientActionResult<object>> IPostEndpoint<object, object>.Post(object model, CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.PostAsClientActionResult<object>(QueryUrl, model, cancellationToken);
    }

    async Task<ClientActionResult<object>> IPostEndpoint<object>.Post(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.PostAsClientActionResult<object>(QueryUrl, null, cancellationToken);
    }

    async Task<ClientActionResult<IEnumerable<object>>> ICustomEndpoint.GetAll(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.GetAsClientActionResult<IEnumerable<object>>(QueryUrl, cancellationToken);
    }
}
