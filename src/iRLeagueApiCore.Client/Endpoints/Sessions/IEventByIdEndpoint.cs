using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Standings;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Sessions;

public interface IEventByIdEndpoint : IUpdateEndpoint<EventModel, PutEventModel>
{
    IEventResultsEndpoint Results();
    IGetAllEndpoint<ReviewModel> Reviews();
    IGetAllEndpoint<MemberModel> Members();
    IEventStandingsEndpoint Standings();
    IGetAllEndpoint<ProtestModel> Protests();
    IGetEndpoint<CarListModel> Cars();
    IGetAllEndpoint<RosterModel> Rosters();
}
