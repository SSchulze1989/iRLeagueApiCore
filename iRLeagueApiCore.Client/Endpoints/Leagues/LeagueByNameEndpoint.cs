using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    internal class LeagueByNameEndpoint : EndpointBase, ILeagueByNameEndpoint
    {
        public string Name { get; }

        public LeagueByNameEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, string leagueName) : 
            base (httpClientWrapper, routeBuilder)
        {
            Name = leagueName;
            RouteBuilder.AddParameter(leagueName);
        }

        ISchedulesEndpoint ILeagueByNameEndpoint.Schedules()
        {
            return new SchedulesEndpoint(HttpClientWrapper, RouteBuilder);
        }

        ISeasonsEndpoint ILeagueByNameEndpoint.Seasons()
        {
            return new SeasonsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        ISessionsEndpoint ILeagueByNameEndpoint.Sessions()
        {
            return new SessionsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IScoringsEndpoint ILeagueByNameEndpoint.Scorings()
        {
            return new ScoringsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}