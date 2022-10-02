using iRLeagueApiCore.Client.Http;
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
    public class PostGetAllEndpoint<TGet, TPost> : EndpointBase, IPostGetAllEndpoint<TGet, TPost>
    {
        public PostGetAllEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
        {
        }

        async Task<ClientActionResult<IEnumerable<TGet>>> IGetEndpoint<IEnumerable<TGet>>.Get(CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.GetAsClientActionResult<IEnumerable<TGet>>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<TGet>> IPostEndpoint<TGet, TPost>.Post(TPost model, CancellationToken cancellationToken)
        {
            return await HttpClientWrapper.PostAsClientActionResult<TGet>(QueryUrl, model, cancellationToken);
        }
    }
}
