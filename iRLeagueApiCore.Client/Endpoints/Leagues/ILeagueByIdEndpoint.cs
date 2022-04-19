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
    public interface ILeagueByIdEndpoint
    {
        public Task<ClientActionResult<GetLeagueModel>> Get(CancellationToken cancellationToken = default);
        public Task<ClientActionResult<GetLeagueModel>> Put(PutLeagueModel model, CancellationToken cancellationToken = default);
        public Task<ClientActionResult<NoContent>> Delete(CancellationToken cancellationToken = default);
    }
}
