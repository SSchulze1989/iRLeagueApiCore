using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public interface ILeagueEndpoint
    {
        public Task<ClientActionResult<IEnumerable<GetLeagueModel>>> GetAll(CancellationToken cancellationToken = default);
        public Task<ClientActionResult<GetLeagueModel>> Get(long id, CancellationToken cancellationToken = default);
        public Task<ClientActionResult<GetLeagueModel>> Post(PostLeagueModel model, CancellationToken cancellationToken = default);
        public Task<ClientActionResult<GetLeagueModel>> Put(long leagueId, PutLeagueModel model, CancellationToken cancellationToken = default);
        public Task<ClientActionResult<NoContent>> Delete(long id, CancellationToken cancellationToken = default);
    }
}
