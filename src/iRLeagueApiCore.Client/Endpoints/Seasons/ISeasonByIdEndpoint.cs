using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Standings;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    public interface ISeasonByIdEndpoint : IUpdateEndpoint<SeasonModel, PutSeasonModel>
    {
        long Id { get; }
        IPostGetAllEndpoint<ScheduleModel, PostScheduleModel> Schedules();
        IPostGetAllEndpoint<ScoringModel, PostScoringModel> Scorings();
        IGetAllEndpoint<SeasonEventResultModel> Results();
        IGetAllEndpoint<EventModel> Events();
        IGetAllEndpoint<StandingsModel> Standings();
    }
}