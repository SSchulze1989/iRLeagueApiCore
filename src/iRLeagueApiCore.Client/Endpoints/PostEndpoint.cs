using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iRLeagueApiCore.Client.Http;

namespace iRLeagueApiCore.Client.Endpoints
{
    public class PostEndpoint<TResult> : EndpointBase, IPostEndpoint<TResult>
    {
        public PostEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) : 
            base(httpClient, routeBuilder)
        {
        }

        public async Task<ClientActionResult<TResult>> Post(CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.PostAsClientActionResult<TResult>(QueryUrl, null, cancellationToken);
        }
    }

    public class PostEndpoint<TResult, TModel> : EndpointBase, IPostEndpoint<TResult, TModel>
    {
        public PostEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
            base(httpClientWrapper, routeBuilder)
        {}

        async Task<ClientActionResult<TResult>> IPostEndpoint<TResult, TModel>.Post(TModel model, CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.PostAsClientActionResult<TResult>(QueryUrl, model, cancellationToken);
        }
    }
}