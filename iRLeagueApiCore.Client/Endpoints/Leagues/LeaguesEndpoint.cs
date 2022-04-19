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
        private readonly ILogger<LeaguesEndpoint> _logger;
        private readonly HttpClient httpClient;

        private const string endpoint = "Leagues";

        public LeaguesEndpoint(ILogger<LeaguesEndpoint> logger, HttpClient httpClient)
        {
            _logger = logger;
            this.httpClient = httpClient;
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

        Task<ClientActionResult<IEnumerable<GetLeagueModel>>> ILeaguesEndpoint.GetAll(CancellationToken cancellationToken)
        {
            return GetAll(cancellationToken);
        }

        Task<ClientActionResult<GetLeagueModel>> ILeaguesEndpoint.Post(PostLeagueModel model, CancellationToken cancellationToken)
        {
            return Post(model, cancellationToken);
        }

        ILeagueByIdEndpoint ILeaguesEndpoint.WithId(long leagueId)
        {
            throw new NotImplementedException();
        }

        ILeagueByNameEndpoint ILeaguesEndpoint.WithName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
