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
    public interface ISeasonsEndpoint : IPostEndpoint<GetSeasonModel, PostSeasonModel>
    {
        public ISeasonByIdEndpoint WitId(long seasonId);
    }
}
