using iRLeagueApiCore.Client.Results;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IGetEndpoint<T>
    {
        Task<ClientActionResult<T>> Get(CancellationToken cancellationToken = default);
    }
}