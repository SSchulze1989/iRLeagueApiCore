using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client
{
    public interface ILeagueApiClient
    {
        bool IsLoggedIn { get; }
        ILeagueByNameEndpoint CurrentLeague { get; }
        ISeasonByIdEndpoint CurrentSeason { get; }
        ILeaguesEndpoint Leagues();
        Task<bool> LogIn(string username, string password, CancellationToken cancellationToken = default);
        Task LogOut();
        void SetCurrentLeague(string leagueName);
        void SetCurrentSeason(string leagueName, long seasonId);
    }
}
