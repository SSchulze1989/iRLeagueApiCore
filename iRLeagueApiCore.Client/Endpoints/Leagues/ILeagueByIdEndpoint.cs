using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public interface ILeagueByIdEndpoint : IUpdateEndpoint<LeagueModel, PutLeagueModel>
    {
    }
}
