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

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonsEndpoint : ISeasonsEndpoint
    {
        private readonly HttpClient httpClient;
        private readonly RouteBuilder routeBuilder;

        private string QueryUrl => routeBuilder.Build();

        public SeasonsEndpoint(HttpClient httpClient, RouteBuilder routeBuilder)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
            routeBuilder.AddEndpoint("Seasons");
        }

        async Task<ClientActionResult<GetSeasonModel>> ISeasonsEndpoint.Post(PostSeasonModel model, CancellationToken cancellationToken)
        {
            return await httpClient.PostAsClientActionResult<GetSeasonModel, PostSeasonModel>(QueryUrl, model, cancellationToken);
        }

        ISeasonByIdEndpoint ISeasonsEndpoint.WitId(long seasonId)
        {
            return new SeasonByIdEndpoint(httpClient, routeBuilder.Copy(), seasonId);
        }
    }
}
