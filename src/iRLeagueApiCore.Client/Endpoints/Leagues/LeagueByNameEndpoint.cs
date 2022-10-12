using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Reviews;
using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.Endpoints.Users;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    internal class LeagueByNameEndpoint : EndpointBase, ILeagueByNameEndpoint
    {
        public string Name { get; }

        string ILeagueByNameEndpoint.Name => throw new System.NotImplementedException();

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

        IEventsEndpoint ILeagueByNameEndpoint.Events()
        {
            return new EventsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IResultConfigsEndpoint ILeagueByNameEndpoint.ResultConfigs()
        {
            return new ResultConfigsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IPointRulesEndpoint ILeagueByNameEndpoint.PointRules()
        {
            return new PointRulesEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IReviewsEndpoint ILeagueByNameEndpoint.Reviews()
        {
            return new ReviewsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IReviewCommentsEndpoint ILeagueByNameEndpoint.ReviewComments()
        {
            return new ReviewCommentsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        ISessionsEndpoint ILeagueByNameEndpoint.Sessions()
        {
            return new SessionsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        ILeagueUsersEndpoint ILeagueByNameEndpoint.Users()
        {
            return new UsersEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}