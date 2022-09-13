using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
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

        IGetAllEndpoint<EventResultModel> ISeasonByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IGetAllEndpoint<EventModel> ISeasonByIdEndpoint.Events()
        {
            return new EventsEndpoint(HttpClientWrapper, RouteBuilder);
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