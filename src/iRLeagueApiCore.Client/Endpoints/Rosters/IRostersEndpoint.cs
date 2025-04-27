using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;
public interface IRostersEndpoint : IGetAllEndpoint<RosterModel>, IPostEndpoint<RosterModel, PostRosterModel>, IWithIdEndpoint<IRosterByIdEndpoint>
{
}
