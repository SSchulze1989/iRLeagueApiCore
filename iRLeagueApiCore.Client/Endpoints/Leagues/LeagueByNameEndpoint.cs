using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    internal class LeagueByNameEndpoint : EndpointBase, ILeagueByNameEndpoint
    {
        public LeagueByNameEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, string leagueName) : 
            base (httpClient, routeBuilder)
        {
            RouteBuilder.AddParameter(leagueName);
        }

        ISchedulesEndpoint ILeagueByNameEndpoint.Schedules()
        {
            return new SchedulesEndpoint(HttpClient, RouteBuilder);
        }

        ISeasonsEndpoint ILeagueByNameEndpoint.Seasons()
        {
            return new SeasonsEndpoint(HttpClient, RouteBuilder);
        }

        ISessionsEndpoint ILeagueByNameEndpoint.Sessions()
        {
            return new SessionsEndpoint(HttpClient, RouteBuilder);
        }

        IScoringsEndpoint ILeagueByNameEndpoint.Scorings()
        {
            return new ScoringsEndpoint(HttpClient, RouteBuilder);
        }
    }
}