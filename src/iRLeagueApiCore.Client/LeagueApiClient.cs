using iRLeagueApiCore.Client.Endpoints;
using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Tracks;
using iRLeagueApiCore.Client.Endpoints.Users;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace iRLeagueApiCore.Client;

public sealed class LeagueApiClient : ILeagueApiClient
{
    private readonly ILogger<LeagueApiClient> logger;
    private readonly HttpClientWrapper httpClientWrapper;
    private readonly ITokenStore tokenStore;

    private string? CurrentLeagueName { get; set; }

    public LeagueApiClient(ILogger<LeagueApiClient> logger, HttpClient httpClient, ITokenStore tokenStore, JsonSerializerOptions jsonOptions)
    {
        this.logger = logger;
        httpClientWrapper = new HttpClientWrapper(httpClient, tokenStore, this, jsonOptions);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        this.tokenStore = tokenStore;
    }

    public bool IsLoggedIn => tokenStore.IsLoggedIn;

    public ILeagueByNameEndpoint? CurrentLeague { get; private set; }
    public ISeasonByIdEndpoint? CurrentSeason { get; private set; }

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

    /// <summary>
    /// Login with username and password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Response with id and access token</returns>
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
            string idToken = result.Content.IdToken;
            string accessToken = result.Content.AccessToken;
            await tokenStore.SetIdTokenAsync(idToken);
            await tokenStore.SetAccessTokenAsync(accessToken);
            return result;
        }

        logger.LogError("Login failed: {Status}", result.Status);
        return result;
    }

    /// <summary>
    /// Get access token using a valid idToken
    /// </summary>
    /// <param name="idToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Response containing access token</returns>
    public async Task<ClientActionResult<AuthorizeResponse>> Authorize(string idToken, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Request access token using id token");

        var requestUrl = "Authenticate/authorize";
        var body = new
        {
            idToken 
        };
        var result = await httpClientWrapper.PostAsClientActionResult<AuthorizeResponse>(requestUrl, body, cancellationToken);

        if (result.Success)
        {
            string token = result.Content.AccessToken;
            await tokenStore.SetAccessTokenAsync(token);
            return result;
        }

        logger.LogError("Access request failed: {Status}", result.Status);
        await LogOut();
        return result;
    }



    public async Task LogOut()
    {
        await tokenStore.ClearTokensAsync();
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
        if (CurrentLeague == null)
        {
            throw new InvalidOperationException("Could not set current season: current league was null");
        }
        CurrentSeason = CurrentLeague.Seasons().WithId(seasonId);
    }

    ICustomEndpoint ILeagueApiClient.CustomEndpoint(string route)
    {
        return new CustomEndpoint(httpClientWrapper, new(), route);
    }
}
