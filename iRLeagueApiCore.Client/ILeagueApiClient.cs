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
        public bool IsLoggedIn { get; }
        public ILeagueByNameEndpoint CurrentLeague { get; }
        public ILeaguesEndpoint Leagues();
        public Task<bool> LogIn(string username, string password, CancellationToken cancellationToken = default);
        public void LogOut();
        public void SetCurrentLeague(string leagueName);
    }
}
