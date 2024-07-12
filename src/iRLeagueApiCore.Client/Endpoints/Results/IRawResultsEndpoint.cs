using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results;
public interface IRawResultsEndpoint
{
    public IGetEndpoint<RawEventResultModel> EventResult(long eventId);
    public IPutEndpoint<RawResultRowModel, RawResultRowModel> ModifyResultRow(long resultRowId, bool triggerCalculation = false);
}
