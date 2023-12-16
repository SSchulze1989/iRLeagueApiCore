using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Results;

public interface ISessionResultByIdEndpoint
{
    IGetAllEndpoint<PenaltyModel> Penalties();
    IGetAllEndpoint<AddBonusModel> Bonuses();
    IWithIdEndpoint<IResultRowByIdEndpoint> Rows();
}