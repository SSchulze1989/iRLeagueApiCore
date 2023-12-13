using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Results;
public interface IResultRowByIdEndpoint
{
    IPostEndpoint<PenaltyModel, PostPenaltyModel> Penalties();
    IPostEndpoint<AddBonusModel, PostAddBonusModel> Bonuses();
}
