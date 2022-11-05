using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal interface IEventResultCalculationResultStore
    {
        public Task StoreCalculationResult(EventResultCalculationResult result, CancellationToken cancellationToken = default);
    }
}