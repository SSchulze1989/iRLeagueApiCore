using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonByIdEndpoint : UpdateEndpoint<GetSeasonModel, PutSeasonModel>, ISeasonByIdEndpoint
    {

        public SeasonByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long seasonId) :
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddParameter(seasonId);
        }

        IGetAllEndpoint<GetResultModel> ISeasonByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClient, RouteBuilder);
        }

        IPostGetAllEndpoint<GetScheduleModel, PostScheduleModel> ISeasonByIdEndpoint.Schedules()
        {
            return new SchedulesEndpoint(HttpClient, RouteBuilder);
        }

        IPostGetAllEndpoint<GetScoringModel, PostScoringModel> ISeasonByIdEndpoint.Scorings()
        {
            return new ScoringsEndpoint(HttpClient, RouteBuilder);
        }
    }
}