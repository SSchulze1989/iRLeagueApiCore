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
    public class LeagueEndpoint : ILeagueEndpoint
    {
        private readonly ILogger<LeagueEndpoint> _logger;
        private readonly HttpClient httpClient;

        private const string endpoint = "Leagues";

        public LeagueEndpoint(ILogger<LeagueEndpoint> logger, HttpClient httpClient)
        {
            _logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<ClientActionResult<NoContent>> Delete(long id, CancellationToken cancellationToken = default)
        {
            var query = new RouteBuilder()
                .AddEndpoint(endpoint)
                .AddParameter(id)
                .Build();
            return await httpClient.DeleteAsClientActionResult(query, cancellationToken);
        }

        public async Task<ClientActionResult<GetLeagueModel>> Get(long id, CancellationToken cancellationToken = default)
        {
            var query = new RouteBuilder()
                .AddEndpoint(endpoint)
                .AddParameter(id)
                .Build();
            return await httpClient.GetAsClientActionResult<GetLeagueModel>(query, cancellationToken);
        }

        public async Task<ClientActionResult<IEnumerable<GetLeagueModel>>> GetAll(CancellationToken cancellationToken = default)
        {
            var query = new RouteBuilder()
                .AddEndpoint(endpoint)
                .Build();
            return await httpClient.GetAsClientActionResult<IEnumerable<GetLeagueModel>>(query, cancellationToken);
        }

        public async Task<ClientActionResult<GetLeagueModel>> Post(PostLeagueModel model, CancellationToken cancellationToken = default)
        {
            var query = new RouteBuilder()
                .AddEndpoint(endpoint)
                .Build();
            return await httpClient.PostAsClientActionResult<GetLeagueModel, PostLeagueModel>(query, model, cancellationToken);
        }

        public async Task<ClientActionResult<GetLeagueModel>> Put(long id, PutLeagueModel model, CancellationToken cancellationToken = default)
        {
            var query = new RouteBuilder()
                .AddEndpoint(endpoint)
                .AddParameter(id)
                .Build();
            return await httpClient.PutAsClientActionResult<GetLeagueModel, PutLeagueModel>(query, model, cancellationToken);
        }
    }
}
