using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public class UpdateEndpoint<TResult, TModel> : EndpointBase, IUpdateEndpoint<TResult, TModel>
    {
        public UpdateEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) : 
            base(httpClient, routeBuilder)
        {
        }

        async Task<ClientActionResult<NoContent>> IDeleteEndpoint.Delete(CancellationToken cancellationToken)
        {
            return await HttpClient.DeleteAsClientActionResult(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<TResult>> IGetEndpoint<TResult>.Get(CancellationToken cancellationToken)
        {
            return await HttpClient.GetAsClientActionResult<TResult>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<TResult>> IPutEndpoint<TResult, TModel>.Put(TModel model, CancellationToken cancellationToken)
        {
            return await HttpClient.PutAsClientActionResult<TResult, TModel>(QueryUrl, model, cancellationToken);
        }
    }
}