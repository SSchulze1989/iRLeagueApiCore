using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IStandingCalculationConfigurationProvider
{
    public Task<long?> GetSeasonId(long eventId, CancellationToken cancellationToken);
    /// <summary>
    /// Returns the id of the next event for which a standing can be calculated. <see langword="null"/> if no further event with results exists
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<long?> GetNextEventId(long eventId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<long>> GetStandingConfigIds(long seasonId, CancellationToken cancellationToken = default);
    public Task<StandingCalculationConfiguration> GetConfiguration(long seasonId, long? eventId, long? resultConfigId, CancellationToken cancellationToken = default);
}
