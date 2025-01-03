using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Standings;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.Sessions;

public interface IEventByIdEndpoint : IUpdateEndpoint<EventModel, PutEventModel>
{
    IEventResultsEndpoint Results();
    IGetAllEndpoint<ReviewModel> Reviews();
    IGetAllEndpoint<MemberModel> Members();
    IEventStandingsEndpoint Standings();
    IGetAllEndpoint<ProtestModel> Protests();
    IGetEndpoint<CarListModel> Cars();
}
