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

        private const string endpoint = "Seasons";

        public SeasonsEndpoint(HttpClient httpClient, RouteBuilder routeBuilder)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
        }

        Task<ClientActionResult<GetSeasonModel>> ISeasonsEndpoint.Post(PostSeasonModel model, CancellationToken cancellationToken)
        {
            return Post(model, cancellationToken);
        }

        ISeasonByIdEndpoint ISeasonsEndpoint.WitId(long seasonId)
        {
            return new SeasonByIdEndpoint(httpClient, routeBuilder);
        }

        public async Task<ClientActionResult<GetSeasonModel>> Post(PostSeasonModel model, CancellationToken cancellationToken = default)
        {
            var queryUrl = routeBuilder
                .AddEndpoint(endpoint)
                .Build();
            return await httpClient.PostAsClientActionResult<GetSeasonModel, PostSeasonModel>(queryUrl, model, cancellationToken);
        }
    }
}
