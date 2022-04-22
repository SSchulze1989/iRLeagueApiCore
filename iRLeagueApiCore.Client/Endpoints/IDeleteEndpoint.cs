using iRLeagueApiCore.Client.Results;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IDeleteEndpoint
    {
        Task<ClientActionResult<NoContent>> Delete(CancellationToken cancellationToken = default);
    }
}