using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    internal class LeagueByNameEndpoint : ILeagueByNameEndpoint
    {
        private readonly HttpClient httpClient;
        private readonly RouteBuilder routeBuilder;

        public LeagueByNameEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, string leagueName)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
            routeBuilder.AddParameter(leagueName);
        }

        ISeasonsEndpoint ILeagueByNameEndpoint.Seasons()
        {
            return new SeasonsEndpoint(httpClient, routeBuilder.Copy());
        }
    }
}