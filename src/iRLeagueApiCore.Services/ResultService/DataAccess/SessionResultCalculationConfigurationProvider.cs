using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal sealed class SessionResultCalculationConfigurationProvider : DatabaseAccessBase
    {
        public SessionResultCalculationConfigurationProvider(ILeagueDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<SessionResultCalculationConfiguration>> GetConfigurations(EventEntity eventEntity,
            ResultConfigurationEntity? configurationEntity, CancellationToken cancellationToken)
        {
            if (configurationEntity == null)
            {
                return DefaultSessionResultCalculationConfigurations(eventEntity);
            }

            // TODO: Implement infer configuration from configEntity
            throw new NotImplementedException();
        }

        private static IEnumerable<SessionResultCalculationConfiguration> DefaultSessionResultCalculationConfigurations(EventEntity eventEntity)
        {
            var configurations = eventEntity.Sessions
                .Select(x => new SessionResultCalculationConfiguration()
                {
                    LeagueId = x.LeagueId,
                    ScoringKind = default,
                    SessionId = x.SessionId,
                    UseResultSetTeam = true,
                    MaxResultsPerGroup = 1,
                    Name = "Default",
                    UpdateTeamOnRecalculation = false,
                });
            return configurations;
        }
    }
}
