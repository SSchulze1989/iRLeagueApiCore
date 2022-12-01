using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IStandingCalculationConfigurationProvider
{
    public Task<StandingCalculationConfiguration> GetConfiguration(long seasonId, long? eventId, long? resultConfigId, CancellationToken cancellationToken = default);
}
