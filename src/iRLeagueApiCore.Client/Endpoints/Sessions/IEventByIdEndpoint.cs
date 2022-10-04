using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface IEventByIdEndpoint : IUpdateEndpoint<EventModel, PutEventModel>
    {
        IGetAllEndpoint<EventResultModel> Results();
        IGetAllEndpoint<ReviewModel> Reviews();
        IGetAllEndpoint<MemberInfoModel> Members();
    }
}