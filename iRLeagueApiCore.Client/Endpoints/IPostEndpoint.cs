using iRLeagueApiCore.Client.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IPostEndpoint<TResult, TModel>
    {
        public Task<ClientActionResult<TResult>> Post(TModel model, CancellationToken cancellationToken = default);
    }
}