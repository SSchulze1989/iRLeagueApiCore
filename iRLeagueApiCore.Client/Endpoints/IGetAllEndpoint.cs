using iRLeagueApiCore.Client.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IGetAllEndpoint<T>
    {
        Task<ClientActionResult<IEnumerable<T>>> GetAll(CancellationToken cancellationToken = default);
    }
}