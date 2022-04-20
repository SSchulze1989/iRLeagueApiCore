using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using Microsoft.AspNetCore.Mvc;
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
    public class LeaguesEndpoint : ILeaguesEndpoint
    {
        private readonly HttpClient httpClient;
        private readonly RouteBuilder routeBuilder;

        private string QueryUrl => routeBuilder.Build();

        public LeaguesEndpoint(HttpClient httpClient, RouteBuilder routeBuilder)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
        }

        async Task<ClientActionResult<IEnumerable<GetLeagueModel>>> ILeaguesEndpoint.Get(CancellationToken cancellationToken)
        {
            routeBuilder.AddEndpoint("Leagues");
            return await httpClient.GetAsClientActionResult<IEnumerable<GetLeagueModel>>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<GetLeagueModel>> ILeaguesEndpoint.Post(PostLeagueModel model, CancellationToken cancellationToken)
        {
            routeBuilder.AddEndpoint("Leagues");
            return await httpClient.PostAsClientActionResult<GetLeagueModel, PostLeagueModel>(QueryUrl, model, cancellationToken);
        }

        ILeagueByIdEndpoint ILeaguesEndpoint.WithId(long leagueId)
        {
            routeBuilder.AddEndpoint("Leagues");
            return new LeagueByIdEndpoint(httpClient, routeBuilder.Copy(), leagueId);
        }

        ILeagueByNameEndpoint ILeaguesEndpoint.WithName(string leagueName)
        {
            return new LeagueByNameEndpoint(httpClient, routeBuilder.Copy(), leagueName);
        }
    }
}
