using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    internal class PostGetAllEndpoint<TGet, TPost> : EndpointBase, IPostGetAllEndpoint<TGet, TPost>
    {
        public PostGetAllEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) : base(httpClient, routeBuilder)
        {
        }

        async Task<ClientActionResult<IEnumerable<TGet>>> IGetEndpoint<IEnumerable<TGet>>.Get(CancellationToken cancellationToken)
        {
            return await HttpClient.GetAsClientActionResult<IEnumerable<TGet>>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<TGet>> IPostEndpoint<TGet, TPost>.Post(TPost model, CancellationToken cancellationToken)
        {
            return await HttpClient.PostAsClientActionResult<TGet, TPost>(QueryUrl, model, cancellationToken);
        }
    }
}
