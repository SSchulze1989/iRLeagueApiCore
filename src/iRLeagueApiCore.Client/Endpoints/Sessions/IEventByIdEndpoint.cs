using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface IEventByIdEndpoint : IUpdateEndpoint<EventModel, PutEventModel>
    {
        IGetAllEndpoint<EventResultModel> Results();
    }
}