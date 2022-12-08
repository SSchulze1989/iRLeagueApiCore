using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iRLeagueApiCore.Client.Http;

namespace iRLeagueApiCore.Client.Endpoints
{
    public class UpdateEndpoint<TResult, TModel> : EndpointBase, IUpdateEndpoint<TResult, TModel> where TModel : notnull
    {
        public UpdateEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
        }

        async Task<ClientActionResult<NoContent>> IDeleteEndpoint.Delete(CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.DeleteAsClientActionResult(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<TResult>> IGetEndpoint<TResult>.Get(CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.GetAsClientActionResult<TResult>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<TResult>> IPutEndpoint<TResult, TModel>.Put(TModel model, CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.PutAsClientActionResult<TResult>(QueryUrl, model, cancellationToken);
        }
    }
}