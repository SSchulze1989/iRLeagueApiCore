using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Client.Endpoints.Rosters;

public interface IRosterEntryByIdEndpoint : IPutEndpoint<RosterEntryModel, RosterEntryModel>, IDeleteEndpoint
{
}
