using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IEventCalculationDataProvider
{
    public Task<EventCalculationData?> GetData(EventCalculationConfiguration config, CancellationToken cancellationToken);
}
