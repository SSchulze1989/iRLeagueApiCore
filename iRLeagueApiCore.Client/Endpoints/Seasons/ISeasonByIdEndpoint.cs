using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    public interface ISeasonByIdEndpoint
    {
        public Task<ClientActionResult<GetSeasonModel>> Get(CancellationToken cancellationToken = default);

        public Task<ClientActionResult<GetSeasonModel>> Put(PutSeasonModel model, CancellationToken cancellationToken = default);

        public Task<ClientActionResult<NoContent>> Delete(CancellationToken cancellationToken = default);
    }
}