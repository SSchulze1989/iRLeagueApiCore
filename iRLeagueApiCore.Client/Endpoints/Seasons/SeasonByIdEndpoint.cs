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
    internal class SeasonByIdEndpoint : UpdateEndpoint<GetSeasonModel, PutSeasonModel>, ISeasonByIdEndpoint
    {
        public long Id { get; }

        public SeasonByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long seasonId) :
            base(httpClientWrapper, routeBuilder)
        {
            Id = seasonId;
            RouteBuilder.AddParameter(seasonId);
        }

        IGetAllEndpoint<GetResultModel> ISeasonByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IGetAllEndpoint<GetSessionModel> ISeasonByIdEndpoint.Sessions()
        {
            return new SessionsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IPostGetAllEndpoint<GetScheduleModel, PostScheduleModel> ISeasonByIdEndpoint.Schedules()
        {
            return new SchedulesEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IPostGetAllEndpoint<GetScoringModel, PostScoringModel> ISeasonByIdEndpoint.Scorings()
        {
            return new ScoringsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}