using iRLeagueApiCore.Communication.Models;

namespace iRLeagueApiCore.Client.Endpoints.Schedules
{
    public interface IScheduleByIdEndpoint : IUpdateEndpoint<GetScheduleModel, PutScheduleModel>
    {
        IPostGetAllEndpoint<SessionModel, PostSessionModel> Sessions();
    }
}