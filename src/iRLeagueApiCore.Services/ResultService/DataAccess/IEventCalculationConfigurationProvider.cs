using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IEventCalculationConfigurationProvider
{
    /// <summary>
    /// Returns the id of the next event for which a result can be calculated. <see langword="null"/> if no further event with results exists
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<long?> GetNextEventId(long eventId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<long>> GetResultConfigIds(long eventId, CancellationToken cancellationToken = default);
    public Task<EventCalculationConfiguration> GetConfiguration(long eventId, long? resultConfigId, CancellationToken cancellationToken = default);
}
