using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Client.Endpoints.Results;

public interface IResultsEndpoint : IWithIdEndpoint<IResultByIdEndpoint>
{
    public IGetEndpoint<IEnumerable<EventResultModel>> Latest();
    public IPutEndpoint<ModRawResultRowModel, ModRawResultRowModel> ModifyResultRow(long resultRowId, bool triggerCalculation = false);
}
