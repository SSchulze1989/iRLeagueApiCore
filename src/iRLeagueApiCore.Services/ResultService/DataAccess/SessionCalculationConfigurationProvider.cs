using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal sealed class SessionCalculationConfigurationProvider : DatabaseAccessBase, ISessionCalculationConfigurationProvider
    {
        public SessionCalculationConfigurationProvider(LeagueDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<SessionCalculationConfiguration>> GetConfigurations(EventEntity eventEntity,
            ResultConfigurationEntity? configurationEntity, CancellationToken cancellationToken = default)
        {
            if (configurationEntity == null)
            {
                return await DefaultSessionResultCalculationConfigurations(eventEntity, configurationEntity, cancellationToken);
            }

            return await GetSessionConfigurationsFromEntity(eventEntity, configurationEntity, cancellationToken);
        }

        private async Task<IEnumerable<SessionCalculationConfiguration>> DefaultSessionResultCalculationConfigurations(EventEntity eventEntity, 
            ResultConfigurationEntity? configurationEntity, CancellationToken cancellationToken)
        {
            var configId = configurationEntity?.ResultConfigId;
            var sessionResultIds = await dbContext.ScoredSessionResults
                .Where(x => x.ScoredEventResult.EventId == eventEntity.EventId)
                .Where(x => x.ScoredEventResult.ResultConfigId == configId)
                .OrderBy(x => x.SessionNr)
                .Select(x => x.SessionResultId)
                .ToListAsync(cancellationToken);
                
            var configurations = eventEntity.Sessions
                .Select((x, i) => new SessionCalculationConfiguration()
                {
                    LeagueId = x.LeagueId,
                    ScoringId = null,
                    SessionResultId = sessionResultIds.ElementAtOrDefault(i),
                    ScoringKind = default,
                    SessionId = x.SessionId,
                    UseResultSetTeam = false,
                    MaxResultsPerGroup = 1,
                    Name = "Default",
                    UpdateTeamOnRecalculation = false,
                });
            return configurations;
        }

        private async Task<IEnumerable<SessionCalculationConfiguration>> GetSessionConfigurationsFromEntity(EventEntity eventEntity,
            ResultConfigurationEntity configurationEntity, CancellationToken cancellationToken)
        {
            var sessionResultIds = await dbContext.ScoredSessionResults
                .Where(x => x.ScoredEventResult.EventId == eventEntity.EventId)
                .Where(x => x.ScoredEventResult.ResultConfigId == configurationEntity.ResultConfigId)
                .Select(x => x.SessionResultId)
                .ToListAsync(cancellationToken);

            var scorings = configurationEntity.Scorings.OrderBy(x => x.Index);
            var raceIndex = 0;
            var sessionConfigurations = new List<SessionCalculationConfiguration>();
            foreach (var session in eventEntity.Sessions)
            {
                var sessionConfiguration = new SessionCalculationConfiguration()
                {
                    LeagueId = session.LeagueId,
                    Name = session.Name,
                    SessionId = session.SessionId,
                };
                var scoring = session.SessionType != SessionType.Race ? null : scorings.ElementAtOrDefault(raceIndex++);
                sessionConfiguration = MapFromScoringEntity(scoring, sessionConfiguration);
                sessionConfigurations.Add(sessionConfiguration);
            }
            return sessionConfigurations;
        }

        private static SessionCalculationConfiguration MapFromScoringEntity(ScoringEntity? scoring, SessionCalculationConfiguration sessionConfiguration)
        {
            if (scoring == null)
            {
                return sessionConfiguration;
            }
            sessionConfiguration.PointRule = GetPointRuleFromEntity(scoring.PointsRule);
            sessionConfiguration.MaxResultsPerGroup = scoring.MaxResultsPerGroup;
            sessionConfiguration.UseResultSetTeam = scoring.UseResultSetTeam;
            sessionConfiguration.UpdateTeamOnRecalculation = scoring.UpdateTeamOnRecalculation;
            sessionConfiguration.ScoringId = scoring.ScoringId;
            sessionConfiguration.ScoringKind = scoring.ScoringKind;

            return sessionConfiguration;
        }

        private static PointRule<ResultRowCalculationResult> GetPointRuleFromEntity(PointRuleEntity pointsRuleEntity)
        {
            CalculationPointRuleBase pointRule;
            if (pointsRuleEntity.PointsPerPlace.Any())
            {
                throw new NotImplementedException();
            }
            else
            {
                pointRule = new MaxPointRule(pointsRuleEntity.MaxPoints, pointsRuleEntity.PointDropOff);
            }

            pointRule.PointSortOptions = pointsRuleEntity.PointsSortOptions;
            pointRule.FinalSortOptions = pointsRuleEntity.FinalSortOptions;

            return pointRule;
        }
    }
}
