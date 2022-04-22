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
    public class GetAllEndpoint<T> : EndpointBase, IGetAllEndpoint<T>
    {
        public GetAllEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) : 
            base(httpClient, routeBuilder)
        {
        }

        async Task<ClientActionResult<IEnumerable<T>>> IGetEndpoint<IEnumerable<T>>.Get(CancellationToken cancellationToken)
        {
            return await HttpClient.GetAsClientActionResult<IEnumerable<T>>(QueryUrl, cancellationToken);
        }
    }
}
