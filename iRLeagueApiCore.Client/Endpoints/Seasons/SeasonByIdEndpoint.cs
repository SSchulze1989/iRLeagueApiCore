using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonByIdEndpoint : UpdateEndpoint<SeasonModel, PutSeasonModel>, ISeasonByIdEndpoint
    {
        public long Id { get; }

        public SeasonByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long seasonId) :
            base(httpClientWrapper, routeBuilder)
        {
            Id = seasonId;
            RouteBuilder.AddParameter(seasonId);
        }

        IGetAllEndpoint<ResultModel> ISeasonByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IGetAllEndpoint<SessionModel> ISeasonByIdEndpoint.Sessions()
        {
            return new SessionsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IPostGetAllEndpoint<ScheduleModel, PostScheduleModel> ISeasonByIdEndpoint.Schedules()
        {
            return new SchedulesEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IPostGetAllEndpoint<ScoringModel, PostScoringModel> ISeasonByIdEndpoint.Scorings()
        {
            return new ScoringsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}