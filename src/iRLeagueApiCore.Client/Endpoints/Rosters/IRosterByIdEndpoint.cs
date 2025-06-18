using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;

public interface IRosterByIdEndpoint : IUpdateEndpoint<RosterModel, PutRosterModel>
{
    IWithIdEndpoint<IRosterEntryByIdEndpoint> Entries();
}