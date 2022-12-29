using iRLeagueApiCore.Client.Endpoints;
using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Teams;
using iRLeagueApiCore.Client.Endpoints.Tracks;
using iRLeagueApiCore.Client.Endpoints.Users;
using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client;

public interface ILeagueApiClient
{
    bool IsLoggedIn { get; }
    ILeagueByNameEndpoint? CurrentLeague { get; }
    ISeasonByIdEndpoint? CurrentSeason { get; }
    ILeaguesEndpoint Leagues();
    IUsersEndpoint Users();
    ITeamsEndpoint Teams();
    ITracksEndpoint Tracks();
    ICustomEndpoint CustomEndpoint(string route);
    Task<ClientActionResult<LoginResponse>> LogIn(string username, string password, CancellationToken cancellationToken = default);
    Task LogOut();
    void SetCurrentLeague(string leagueName);
    void SetCurrentSeason(string leagueName, long seasonId);
}
