using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Tracks;
using iRLeagueApiCore.Client.Endpoints.Users;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client
{
    public class LeagueApiClient : ILeagueApiClient
    {
        private readonly ILogger<LeagueApiClient> logger;
        private readonly HttpClientWrapper httpClientWrapper;
        private readonly ITokenStore tokenStore;

        private string CurrentLeagueName { get; set; }

        public LeagueApiClient(ILogger<LeagueApiClient> logger, HttpClient httpClient, ITokenStore tokenStore, JsonSerializerOptions jsonOptions)
        {
            this.logger = logger;
            this.httpClientWrapper = new HttpClientWrapper(httpClient, tokenStore, this, jsonOptions);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.tokenStore = tokenStore;
        }

        public bool IsLoggedIn => tokenStore.IsLoggedIn;

        public ILeagueByNameEndpoint CurrentLeague { get; private set; }
        public ISeasonByIdEndpoint CurrentSeason { get; private set; }

        public ILeaguesEndpoint Leagues()
        {
            return new LeaguesEndpoint(httpClientWrapper, new RouteBuilder());
        }

        public IUsersEndpoint Users()
        {
            return new UsersEndpoint(httpClientWrapper, new RouteBuilder());
        }

        public ITracksEndpoint Tracks()
        {
            return new TracksEndpoint(httpClientWrapper, new RouteBuilder());
        }

        public async Task<ClientActionResult<LoginResponse>> LogIn(string username, string password, CancellationToken cancellationToken = default)
        {
            // request to login endpoint
            await LogOut();

            logger.LogInformation("Log in for {User} ...", username);
            var requestUrl = "Authenticate/Login";
            var body = new
            {
                username = username,
                password = password
            };
            var result = await httpClientWrapper.PostAsClientActionResult<LoginResponse>(requestUrl, body, cancellationToken);

            if (result.Success)
            {
                logger.LogInformation("Log in successful!");
                // set authorization header
                string token = result.Content.Token;
                await tokenStore.SetTokenAsync(token);
                return result;
            }

            logger.LogError("Login failed: {Status}", result.Status);
            return result;
        }

        public async Task LogOut()
        {
            await tokenStore.ClearTokenAsync();
            logger.LogInformation("User logged out");
        }

        public void SetCurrentLeague(string leagueName)
        {
            CurrentLeagueName = leagueName;
            if (string.IsNullOrEmpty(CurrentLeagueName))
            {
                CurrentLeague = null;
                return;
            }

            CurrentLeague = Leagues().WithName(leagueName);
        }

        public void SetCurrentSeason(string leagueName, long seasonId)
        {
            SetCurrentLeague(leagueName);
            CurrentSeason = CurrentLeague.Seasons().WithId(seasonId);
        }
    }
}
