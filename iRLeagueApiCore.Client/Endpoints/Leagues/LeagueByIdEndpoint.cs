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
        private readonly ILogger<LeagueByIdEndpoint> _logger;
        private readonly HttpClient httpClient;
        private readonly RouteBuilder routeBuilder;

        public long Id { get; }

        public LeagueByIdEndpoint(ILogger<LeagueByIdEndpoint> logger, HttpClient httpClient, RouteBuilder routeBuilder, long id)
        {
            _logger = logger;
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
            Id = id;
        }

        Task<ClientActionResult<NoContent>> ILeagueByIdEndpoint.Delete(CancellationToken cancellationToken)
        {
            return Delete(cancellationToken);
        }

        Task<ClientActionResult<GetLeagueModel>> ILeagueByIdEndpoint.Get(CancellationToken cancellationToken)
        {
            return Get(cancellationToken);
        }

        Task<ClientActionResult<GetLeagueModel>> ILeagueByIdEndpoint.Put(PutLeagueModel model, CancellationToken cancellationToken)
        {
            return Put(model, cancellationToken);
        }

        private async Task<ClientActionResult<NoContent>> Delete(CancellationToken cancellationToken = default)
        {
            var query = routeBuilder.Build();
            return await httpClient.DeleteAsClientActionResult(query, cancellationToken);
        }

        private async Task<ClientActionResult<GetLeagueModel>> Get(CancellationToken cancellationToken = default)
        {
            var query = routeBuilder.Build();
            return await httpClient.GetAsClientActionResult<GetLeagueModel>(query, cancellationToken);
        }

        public async Task<ClientActionResult<GetLeagueModel>> Put(PutLeagueModel model, CancellationToken cancellationToken = default)
        {
            var query = routeBuilder.Build();
            return await httpClient.PutAsClientActionResult<GetLeagueModel, PutLeagueModel>(query, model, cancellationToken);
        }
    }
}
