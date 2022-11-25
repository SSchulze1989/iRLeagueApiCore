﻿using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal sealed class EventCalculationConfigurationProvider : DatabaseAccessBase, IEventCalculationConfigurationProvider
    {
        private readonly ISessionCalculationConfigurationProvider sessionConfigurationProvider;
        public EventCalculationConfigurationProvider(LeagueDbContext dbContext, 
            ISessionCalculationConfigurationProvider sessionConfigurationProvider) : 
            base(dbContext)
        {
            this.sessionConfigurationProvider = sessionConfigurationProvider;
        }

        public async Task<IReadOnlyList<long>> GetResultConfigIds(long eventId, CancellationToken cancellationToken = default)
        {
            var configs = await dbContext.Events
                .Where(x => x.EventId == eventId)
                .SelectMany(x => x.ResultConfigs.Select(y => new { y.ResultConfigId, y.SourceResultConfigId }))
                .ToListAsync(cancellationToken);
            return SortInOrderOfDependency(configs.Select(x => (x.ResultConfigId, x.SourceResultConfigId)));
        }

        public async Task<EventCalculationConfiguration> GetConfiguration(long eventId, long? resultConfigId, CancellationToken cancellationToken = default)
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
                    .Include(x => x.Sessions)
                    .Where(x => x.EventId == eventId)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException($"No event id:{eventId} found in database");
            }

            return await dbContext.Events
                .Include(x => x.Sessions)
                .Where(x => x.EventId == eventId)
                .Where(x => x.ResultConfigs.Any(y => y.ResultConfigId == resultConfigId))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException($"No event id:{eventId} registered with result configuration id:{resultConfigId}");
        }

        private async Task<EventCalculationConfiguration> GetEventResultCalculationConfiguration(EventEntity eventEntity, ResultConfigurationEntity? configEntity,
            CancellationToken cancellationToken)
        {
            EventCalculationConfiguration configuration = new();
            var configId = configEntity?.ResultConfigId;
            configuration.EventId = eventEntity.EventId;
            configuration.LeagueId = eventEntity.LeagueId;
            configuration.ResultId = await dbContext.ScoredEventResults
                .Where(x => x.ResultConfigId == configId)
                .Where(x => x.EventId == eventEntity.EventId)
                .Select(x => x.ResultId)
                .FirstOrDefaultAsync(cancellationToken);
            configuration.ResultConfigId = configId;
            configuration.SourceResultConfigId = configEntity?.SourceResultConfigId;
            configuration.DisplayName = configEntity?.DisplayName ?? "Default";
            configuration.SessionResultConfigurations = await sessionConfigurationProvider.GetConfigurations(eventEntity, configEntity, cancellationToken);
            return configuration;
        }

        private IReadOnlyList<long> SortInOrderOfDependency(IEnumerable<(long id, long? sourceId)> configs)
        {
            var sortList = new List<long>();
            if (configs.Any() == false)
            {
                return sortList;
            }

            var source = configs.ToDictionary(k => k.id, v => v.sourceId);
            var startNodes = configs.Where(x => x.sourceId is null).ToList();
            if (startNodes.Any() == false || startNodes.Any(x => x.id == x.sourceId))
            {
                throw new InvalidOperationException("ResultConfiguration list contains cyclic dependencies");
            }

            while(startNodes.Any())
            {
                var node = startNodes.First();
                startNodes.Remove(node);

                sortList.Add(node.id);
                foreach(var (id, sourceId) in source.Where(x => x.Value == node.id))
                {
                    source[id] = null;
                    startNodes.Add((id, null));
                }
            }

            if (source.Values.Any(x => x is not null))
            {
                throw new InvalidOperationException("ResultConfiguration list contains cyclic dependencies, or dependencies outside of this event scope");
            }

            return sortList;
        }
    }
}
