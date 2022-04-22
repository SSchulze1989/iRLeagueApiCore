using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
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
    public class LeagueByIdEndpoint : UpdateEndpoint<GetLeagueModel, PutLeagueModel>, ILeagueByIdEndpoint
    {
        public LeagueByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long leagueId) : 
            base (httpClient, routeBuilder)
        {
            RouteBuilder.AddParameter(leagueId);
        }
    }
}
