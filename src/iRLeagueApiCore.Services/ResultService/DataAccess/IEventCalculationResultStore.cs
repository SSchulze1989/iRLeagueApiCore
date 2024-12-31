using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IEventCalculationResultStore
{
    public Task StoreCalculationResult(EventCalculationResult result, CancellationToken cancellationToken = default);
    /// <summary>
    /// Remove results that are not in the listed resultConfigIds
    /// </summary>
    /// <param name="resultConfigIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<long>> PruneResults(long eventId, IEnumerable<long?> resultConfigIds, CancellationToken cancellationToken = default);
}
