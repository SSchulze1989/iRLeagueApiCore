using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    public interface ISeasonByIdEndpoint : IUpdateEndpoint<SeasonModel, PutSeasonModel>
    {
        long Id { get; }
        IPostGetAllEndpoint<ScheduleModel, PostScheduleModel> Schedules();
        IPostGetAllEndpoint<ScoringModel, PostScoringModel> Scorings();
        IGetAllEndpoint<ResultModel> Results();
        IGetAllEndpoint<SessionModel> Sessions();
    }
}