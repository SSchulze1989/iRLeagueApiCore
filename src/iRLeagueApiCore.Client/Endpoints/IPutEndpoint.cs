using iRLeagueApiCore.Client.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IPutEndpoint<TResult, TModel>
    {
        Task<ClientActionResult<TResult>> Put(TModel model, CancellationToken cancellationToken = default);
    }
}