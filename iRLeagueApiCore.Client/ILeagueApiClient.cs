using iRLeagueApiCore.Client.Endpoints.Leagues;
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
        ILeaguesEndpoint Leagues();
        Task<bool> LogIn(string username, string password, CancellationToken cancellationToken = default);
        void LogOut();
        void SetCurrentLeague(string leagueName);
    }
}
