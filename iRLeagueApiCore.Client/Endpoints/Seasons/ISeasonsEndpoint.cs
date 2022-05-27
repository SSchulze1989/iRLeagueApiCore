using iRLeagueApiCore.Client.Endpoints.Schedules;
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
    public interface ISeasonsEndpoint : IPostEndpoint<SeasonModel, PostSeasonModel>, IGetAllEndpoint<SeasonModel>, IWithIdEndpoint<ISeasonByIdEndpoint>
    {
    }
}
