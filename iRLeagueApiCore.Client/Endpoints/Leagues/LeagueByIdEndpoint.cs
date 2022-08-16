using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public class LeagueByIdEndpoint : UpdateEndpoint<LeagueModel, PutLeagueModel>, ILeagueByIdEndpoint
    {
        public LeagueByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long leagueId) : 
            base (httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddParameter(leagueId);
        }
    }
}
