using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public class PostEndpoint<TResult, TModel> : EndpointBase, IPostEndpoint<TResult, TModel>
    {
        public PostEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) :
            base(httpClient, routeBuilder)
        {}

        async Task<ClientActionResult<TResult>> IPostEndpoint<TResult, TModel>.Post(TModel model, CancellationToken cancellationToken)
        {
            return await HttpClient.PostAsClientActionResult<TResult, TModel>(QueryUrl, model, cancellationToken);
        }
    }
}