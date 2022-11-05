using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal interface IEventResultCalculationDataProvider
    {
        public Task<EventResultCalculationData?> GetData(EventResultCalculationConfiguration config, CancellationToken cancellationToken);
    }
}