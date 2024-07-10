using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints;

internal class PutEndpoint<TResult, TModel> : EndpointBase, IPutEndpoint<TResult, TModel> where TModel : notnull
{
    public PutEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    { }

    async Task<ClientActionResult<TResult>> IPutEndpoint<TResult, TModel>.Put(TModel model, CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.PostAsClientActionResult<TResult>(QueryUrl, model, cancellationToken);
    }
}
