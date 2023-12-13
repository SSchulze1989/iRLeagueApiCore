using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Bonuses;
public interface IBonusesEndpoint : IWithIdEndpoint<IUpdateEndpoint<AddBonusModel, PutAddBonusModel>>
{
}
