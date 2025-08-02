using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface ISessionCalculationConfigurationProvider
{
    public Task<IEnumerable<SessionCalculationConfiguration>> GetConfigurations(EventEntity eventEntity,
        PointSystemEntity? configurationEntity, CancellationToken cancellationToken);
}
