using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;
public interface IRostersEndpoint : IGetAllEndpoint<RosterInfoModel>, IPostEndpoint<RosterModel, PostRosterModel>, IWithIdEndpoint<IRosterByIdEndpoint>
{
}
