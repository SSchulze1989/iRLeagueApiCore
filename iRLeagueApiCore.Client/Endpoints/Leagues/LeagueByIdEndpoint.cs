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
    public class LeagueByIdEndpoint : ILeagueByIdEndpoint
    {
        private readonly HttpClient httpClient;
        private readonly RouteBuilder routeBuilder;

        private string QueryUrl => routeBuilder.Build();

        public LeagueByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long leagueId)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
            routeBuilder.AddParameter(leagueId);
        }

        async Task<ClientActionResult<NoContent>> ILeagueByIdEndpoint.Delete(CancellationToken cancellationToken)
        {
            return await httpClient.DeleteAsClientActionResult(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<GetLeagueModel>> ILeagueByIdEndpoint.Get(CancellationToken cancellationToken)
        {
            return await httpClient.GetAsClientActionResult<GetLeagueModel>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<GetLeagueModel>> ILeagueByIdEndpoint.Put(PutLeagueModel model, CancellationToken cancellationToken)
        {
            return await httpClient.PutAsClientActionResult<GetLeagueModel, PutLeagueModel>(QueryUrl, model, cancellationToken);
        }
    }
}
