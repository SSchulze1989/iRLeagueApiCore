using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public interface ILeaguesEndpoint
    { 
        public Task<ClientActionResult<IEnumerable<GetLeagueModel>>> GetAll(CancellationToken cancellationToken = default);
        public Task<ClientActionResult<GetLeagueModel>> Post(PostLeagueModel model, CancellationToken cancellationToken = default);
        public ILeagueByIdEndpoint WithId(long leagueId);
        public ILeagueByNameEndpoint WithName(string name); 
    }
}
