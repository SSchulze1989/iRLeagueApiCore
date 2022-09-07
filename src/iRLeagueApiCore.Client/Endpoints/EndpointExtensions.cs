using iRLeagueApiCore.Client.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public static class EndpointExtensions
    {
        /// <summary>
        /// Extension for Post requests with empty request body
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="postEndpoint"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<ClientActionResult<TResult>> Post<TResult>(this IPostEndpoint<TResult, NoContent> postEndpoint, CancellationToken cancellationToken = default)
        {
            return postEndpoint.Post(default, cancellationToken);
        }
    }
}
