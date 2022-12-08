using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IStandingCalculationDataProvider
{
    public Task<StandingCalculationData?> GetData(StandingCalculationConfiguration config, CancellationToken cancellationToken = default);
}
