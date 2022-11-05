using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal interface ISessionResultCalculationConfigurationProvider
    {
        public Task<IEnumerable<SessionResultCalculationConfiguration>> GetConfigurations(EventEntity eventEntity,
            ResultConfigurationEntity? configurationEntity, CancellationToken cancellationToken);
    }
}