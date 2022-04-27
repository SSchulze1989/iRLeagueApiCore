using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client
{
    public class LeagueApiClient : ILeagueApiClient
    {
        private readonly ILogger<LeagueApiClient> logger;
        private readonly HttpClient httpClient;
        private string CurrentLeagueName { get; set; }

        public LeagueApiClient(ILogger<LeagueApiClient> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public bool IsLoggedIn => throw new NotImplementedException();

        public ILeagueByNameEndpoint CurrentLeague => Leagues().WithName(CurrentLeagueName);

        public ILeaguesEndpoint Leagues()
        {
            return new LeaguesEndpoint(httpClient, new RouteBuilder());
        }

        public async Task<bool> LogIn(string username, string password, CancellationToken cancellationToken = default)
        {
            // request to login endpoint
            logger.LogInformation("Log in for {User} ...", username);
            var requestUrl = "Authenticate/Login";
            var body = new
            {
                username = username,
                password = password
            };
            var result = await httpClient.PostAsClientActionResult<LoginResponse, object>(requestUrl, body, cancellationToken);

            if (result.Success)
            {
                logger.LogInformation("Log in successful!");
                // set authorization header
                string token = result.Content.Token;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return true;
            }

            logger.LogError("Login failed: {Status}", result.Status);
            return false;
        }

        public void LogOut()
        {
            httpClient.DefaultRequestHeaders.Authorization = default;
        }

        public void SetCurrentLeague(string leagueName)
        {
            CurrentLeagueName = leagueName;
        }

        private struct LoginResponse
        {
            public string Token { get; set; }
            public DateTime Expires { get; set; }
        }
    }
}
