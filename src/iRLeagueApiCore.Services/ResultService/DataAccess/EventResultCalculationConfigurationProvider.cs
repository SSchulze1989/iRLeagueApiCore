using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal sealed class EventResultCalculationConfigurationProvider : DatabaseAccessBase
    {
        private readonly SessionResultCalculationConfigurationProvider sessionConfigurationProvider;
        public EventResultCalculationConfigurationProvider(ILeagueDbContext dbContext, SessionResultCalculationConfigurationProvider sessionConfigurationProvider) : 
            base(dbContext)
        {
            this.sessionConfigurationProvider = sessionConfigurationProvider;
        }

        public async Task<EventResultCalculationConfiguration> GetConfiguration(long eventId, long? resultConfigId, CancellationToken cancellationToken = default)
        {
            var configEntity = await GetResultConfigurationEntity(resultConfigId, cancellationToken);
            var eventEntity = await GetEventEntity(eventId, resultConfigId, cancellationToken);
            var resultConfiguration = await GetEventResultCalculationConfiguration(eventEntity, configEntity, cancellationToken);
            return resultConfiguration;
        }

        private async Task<ResultConfigurationEntity?> GetResultConfigurationEntity(long? resultConfigId, CancellationToken cancellationToken)
        {
            if (resultConfigId == null)
            {
                return null;
            }

            return await dbContext.ResultConfigurations
                .Include(x => x.Scorings)
                    .ThenInclude(x => x.PointsRule)
                .Include(x => x.Scorings)
                    .ThenInclude(x => x.ExtScoringSource)
                .FirstOrDefaultAsync(x => x.ResultConfigId == resultConfigId, cancellationToken)
                ?? throw new InvalidOperationException($"No result configuration with id:{resultConfigId} found");
        }

        private async Task<EventEntity> GetEventEntity(long eventId, long? resultConfigId, CancellationToken cancellationToken)
        {
            if (resultConfigId == null)
            {
                return await dbContext.Events
                    .Where(x => x.EventId == eventId)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException($"No event id:{eventId} found in database");
            }

            return await dbContext.Events
                .Where(x => x.EventId == eventId)
                .Where(x => x.ResultConfigs.Any(y => y.ResultConfigId == resultConfigId))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException($"No event id:{eventId} registered with result configuration id:{resultConfigId}");
        }

        private async Task<EventResultCalculationConfiguration> GetEventResultCalculationConfiguration(EventEntity eventEntity, ResultConfigurationEntity? configEntity,
            CancellationToken cancellationToken)
        {
            EventResultCalculationConfiguration configuration = new();
            configuration.EventId = eventEntity.EventId;
            configuration.LeagueId = eventEntity.LeagueId;
            configuration.ResultConfigId = configEntity?.ResultConfigId;
            configuration.SessionResultConfigurations = await sessionConfigurationProvider.GetConfigurations(eventEntity, configEntity, cancellationToken);
            return configuration;
        }
    }
}
