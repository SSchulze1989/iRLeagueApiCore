using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints;

internal class PostGetAllEndpoint<TGetAll, TGetPost, TPost> : EndpointBase, IPostGetAllEndpoint<TGetAll, TGetPost, TPost> where TPost : notnull
{
    public PostGetAllEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
    {
    }

    async Task<ClientActionResult<IEnumerable<TGetAll>>> IGetEndpoint<IEnumerable<TGetAll>>.Get(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.GetAsClientActionResult<IEnumerable<TGetAll>>(QueryUrl, cancellationToken);
    }

    async Task<ClientActionResult<TGetPost>> IPostEndpoint<TGetPost, TPost>.Post(TPost model, CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.PostAsClientActionResult<TGetPost>(QueryUrl, model, cancellationToken);
    }
}

internal class PostGetAllEndpoint<TGet, TPost> : PostGetAllEndpoint<TGet, TGet, TPost>, IPostGetAllEndpoint<TGet, TPost> where TPost : notnull
{
    public PostGetAllEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
    {
    }
}
