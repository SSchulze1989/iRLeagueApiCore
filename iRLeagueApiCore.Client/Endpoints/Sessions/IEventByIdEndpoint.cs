using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface IEventByIdEndpoint : IUpdateEndpoint<EventModel, PutEventModel>
    {
        IGetAllEndpoint<EventResultModel> Results();
    }
}