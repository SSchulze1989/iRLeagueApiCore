using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    public interface ISeasonsEndpoint
    {
        public Task<ClientActionResult<GetSeasonModel>> Post(PostSeasonModel model, CancellationToken cancellationToken = default);
        public ISeasonByIdEndpoint WitId(long seasonId);
    }
}
