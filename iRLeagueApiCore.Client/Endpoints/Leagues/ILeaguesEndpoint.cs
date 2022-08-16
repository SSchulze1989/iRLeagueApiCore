using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public interface ILeaguesEndpoint : IGetAllEndpoint<LeagueModel>
    {
        Task<ClientActionResult<LeagueModel>> Post(PostLeagueModel model, CancellationToken cancellationToken = default);
        ILeagueByIdEndpoint WithId(long leagueId);
        ILeagueByNameEndpoint WithName(string name); 
    }
}
