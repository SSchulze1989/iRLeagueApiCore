using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    public interface ISeasonByIdEndpoint : IUpdateEndpoint<GetSeasonModel, PutSeasonModel>
    {
        IPostGetAllEndpoint<GetScheduleModel, PostScheduleModel> Schedules();
        IPostGetAllEndpoint<GetScoringModel, PostScoringModel> Scorings();
        IGetAllEndpoint<GetResultModel> Results();
    }
}