using iRLeagueApiCore.Communication.Models;

namespace iRLeagueApiCore.Client.Endpoints.Schedules
{
    public interface IScheduleByIdEndpoint : IUpdateEndpoint<GetScheduleModel, PutScheduleModel>
    {
        IPostEndpoint<GetSessionModel, PostSessionModel> Sessions();
    }
}