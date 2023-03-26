using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

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
            .OrderBy(x => x.SessionNr)
            .Select((x, i) => new SessionCalculationConfiguration()
            {
                LeagueId = x.LeagueId,
                ScoringId = null,
                SessionResultId = sessionResultIds.ElementAtOrDefault(i),
                SessionId = x.SessionId,
                SessionNr = x.SessionNr,
                UseResultSetTeam = false,
                MaxResultsPerGroup = configurationEntity?.ResultsPerTeam ?? 1,
                Name = x.Name,
                UpdateTeamOnRecalculation = false,
                ResultKind = configurationEntity?.ResultKind ?? ResultKind.Member,
            });
        return configurations;
    }

    private async Task<IEnumerable<SessionCalculationConfiguration>> GetSessionConfigurationsFromEntity(EventEntity eventEntity,
        ResultConfigurationEntity configurationEntity, CancellationToken cancellationToken)
    {
        var sessionResultIds = await dbContext.ScoredSessionResults
            .Where(x => x.ScoredEventResult.EventId == eventEntity.EventId)
            .Where(x => x.ScoredEventResult.ResultConfigId == configurationEntity.ResultConfigId)
            .OrderBy(x => x.SessionNr)
            .Select(x => x.SessionResultId)
            .ToListAsync(cancellationToken);

        var scorings = configurationEntity.Scorings
            .Where(x => x.IsCombinedResult == false)
            .OrderBy(x => x.Index);
        var raceIndex = scorings.Count() - eventEntity.Sessions.Count(x => x.SessionType == SessionType.Race);
        var sessionConfigurations = new List<SessionCalculationConfiguration>();
        foreach ((var session, var index) in eventEntity.Sessions
            .OrderBy(x => x.SessionNr)
            .Select((x, i) => (x, i)))
        {
            var sessionConfiguration = new SessionCalculationConfiguration()
            {
                LeagueId = session.LeagueId,
                Name = session.Name,
                SessionId = session.SessionId,
                SessionNr = session.SessionNr,
                ResultKind = configurationEntity.ResultKind,
            };
            var scoring = session.SessionType != SessionType.Race ? null : scorings.ElementAtOrDefault(raceIndex++);
            sessionConfiguration.SessionType = session.SessionType;
            sessionConfiguration.SessionResultId = sessionResultIds.ElementAtOrDefault(index);
            sessionConfiguration = MapFromScoringEntity(scoring, configurationEntity, sessionConfiguration);
            sessionConfigurations.Add(sessionConfiguration);
        }
        var combinedScoring = configurationEntity.Scorings.FirstOrDefault(x => x.IsCombinedResult);
        if (combinedScoring != null)
        {
            var sessionConfiguration = new SessionCalculationConfiguration()
            {
                LeagueId = configurationEntity.LeagueId,
                Name = combinedScoring.Name,
                SessionId = null,
                SessionNr = 999,
                ResultKind = configurationEntity.ResultKind,
                IsCombinedResult = true,
                UseExternalSourcePoints = combinedScoring.UseExternalSourcePoints,
            };
            sessionConfiguration.SessionType = SessionType.Race;
            sessionConfiguration.SessionResultId = null;
            sessionConfiguration = MapFromScoringEntity(combinedScoring, configurationEntity, sessionConfiguration, includePointFilters: false);
            sessionConfigurations.Add(sessionConfiguration);
        }
        return sessionConfigurations;
    }

    private static SessionCalculationConfiguration MapFromScoringEntity(ScoringEntity? scoring, ResultConfigurationEntity configurationEntity,
        SessionCalculationConfiguration sessionConfiguration, bool includePointFilters = true)
    {
        sessionConfiguration.PointRule = GetPointRuleFromEntity(scoring?.PointsRule, configurationEntity, includePointFilters: includePointFilters);
        sessionConfiguration.MaxResultsPerGroup = configurationEntity.ResultsPerTeam;
        sessionConfiguration.UseResultSetTeam = scoring?.UseResultSetTeam ?? false;
        sessionConfiguration.UpdateTeamOnRecalculation = scoring?.UpdateTeamOnRecalculation ?? false;
        sessionConfiguration.ScoringId = scoring?.ScoringId;

        return sessionConfiguration;
    }

    private static PointRule<ResultRowCalculationResult> GetPointRuleFromEntity(PointRuleEntity? pointsRuleEntity, ResultConfigurationEntity configurationEntity,
        bool includePointFilters = true)
    {
        CalculationPointRuleBase pointRule;

        if (pointsRuleEntity?.PointsPerPlace.Any() == true)
        {
            pointRule = new PerPlacePointRule(PointsPerPlaceToDictionary<double>(pointsRuleEntity.PointsPerPlace
                .Select(x => (double)x)));
        }
        else if (pointsRuleEntity?.MaxPoints > 0)
        {
            pointRule = new MaxPointRule(pointsRuleEntity.MaxPoints, pointsRuleEntity.PointDropOff);
        }
        else
        {
            pointRule = new UseResultPointsPointRule();
        }

        pointRule.PointSortOptions = pointsRuleEntity?.PointsSortOptions ?? Array.Empty<SortOptions>();
        pointRule.FinalSortOptions = pointsRuleEntity?.FinalSortOptions ?? Array.Empty<SortOptions>();
        pointRule.BonusPoints = pointsRuleEntity?.BonusPoints ?? new Dictionary<string, int>();
        pointRule.ResultFilters = MapFromFilterEntities(configurationEntity.ResultFilters);
        if (includePointFilters)
        {
            pointRule.PointFilters = MapFromFilterEntities(configurationEntity.PointFilters);
        }

        return pointRule;
    }

    private static IEnumerable<RowFilter<ResultRowCalculationResult>> MapFromFilterEntities(ICollection<FilterOptionEntity> pointFilters)
    {
        return pointFilters.Select(x => x.Conditions.FirstOrDefault())
            .Select(GetRowFilterFromCondition)
            .NotNull();
    }

    private static IReadOnlyDictionary<int, T> PointsPerPlaceToDictionary<T>(IEnumerable<T> points)
    {
        return points
            .Select((x, i) => new { pos = i + 1, value = x })
            .ToDictionary(k => k.pos, v => v.value);
    }

    private static RowFilter<ResultRowCalculationResult>? GetRowFilterFromCondition(FilterConditionEntity? condition)
    {
        return condition?.FilterType switch
        {
            FilterType.ColumnProperty => new ColumnValueRowFilter(condition.ColumnPropertyName, condition.Comparator, condition.FilterValues, condition.Action),
            FilterType.Member => new MemberRowFilter(condition.FilterValues, condition.Action),
            _ => null,
        };
    }
}
