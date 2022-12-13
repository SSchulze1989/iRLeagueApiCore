using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public class ResultConfigHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{
    public ResultConfigHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<ResultConfigurationEntity?> GetResultConfigEntity(long leagueId, long resultConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultConfigurations
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
            .Include(x => x.SourceResultConfig)
            .Include(x => x.ResultFilters)
                .ThenInclude(x => x.Conditions)
            .Include(x => x.PointFilters)
                .ThenInclude(x => x.Conditions)
            .Include(x => x.StandingConfigurations)
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ResultConfigId == resultConfigId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual async Task<ResultConfigurationEntity> MapToResultConfigEntityAsync(LeagueUser user, PostResultConfigModel postResultConfig,
        ResultConfigurationEntity resultConfigEntity, CancellationToken cancellationToken)
    {
        resultConfigEntity.DisplayName = postResultConfig.DisplayName;
        resultConfigEntity.SourceResultConfig = postResultConfig.SourceResultConfig != null
            ? await dbContext.ResultConfigurations
                .FirstOrDefaultAsync(x => x.ResultConfigId == postResultConfig.SourceResultConfig.ResultConfigId, cancellationToken)
            : null;
        resultConfigEntity.Name = postResultConfig.Name;
        resultConfigEntity.ResultKind = postResultConfig.ResultKind;
        resultConfigEntity.ResultsPerTeam = postResultConfig.ResultsPerTeam;
        resultConfigEntity.StandingConfigurations = await MapToStandingConfigListAsync(resultConfigEntity.LeagueId, user, postResultConfig.StandingConfig, 
            resultConfigEntity.StandingConfigurations, cancellationToken);
        resultConfigEntity.Scorings = await MapToScoringList(resultConfigEntity.LeagueId, user, postResultConfig.Scorings, resultConfigEntity.Scorings, cancellationToken);
        resultConfigEntity.PointFilters = await MapToFilterOptionListAsync(resultConfigEntity.LeagueId, user, postResultConfig.FiltersForPoints,
            resultConfigEntity.PointFilters, cancellationToken);
        resultConfigEntity.ResultFilters = await MapToFilterOptionListAsync(resultConfigEntity.LeagueId, user, postResultConfig.FiltersForResult,
            resultConfigEntity.ResultFilters, cancellationToken);
        UpdateVersionEntity(user, resultConfigEntity);
        return await Task.FromResult(resultConfigEntity);
    }

    private async Task<ICollection<StandingConfigurationEntity>> MapToStandingConfigListAsync(long leagueId, LeagueUser user, StandingConfigModel? standingConfigModel, 
        ICollection<StandingConfigurationEntity> standingConfigurationEntities, CancellationToken cancellationToken)
    {
        if (standingConfigModel is null)
        {
            return Array.Empty<StandingConfigurationEntity>().ToList();
        }

        var standingConfigEntity = standingConfigurationEntities.FirstOrDefault(x => x.StandingConfigId == standingConfigModel.StandingConfigId);
        if (standingConfigEntity is null)
        {
            standingConfigEntity = CreateVersionEntity(user, new StandingConfigurationEntity());
            standingConfigEntity.LeagueId = leagueId;
            standingConfigurationEntities.Clear();
            standingConfigurationEntities.Add(standingConfigEntity);
        }
        standingConfigEntity.Name = standingConfigModel.Name;
        standingConfigEntity.ResultKind = standingConfigModel.ResultKind;
        standingConfigEntity.UseCombinedResult = standingConfigModel.UseCombinedResult;
        standingConfigEntity.WeeksCounted = standingConfigModel.WeeksCounted;
        UpdateVersionEntity(user, standingConfigEntity);
        return await Task.FromResult(standingConfigurationEntities);
    }

    private async Task<ICollection<FilterOptionEntity>> MapToFilterOptionListAsync(long leagueId, LeagueUser user, IEnumerable<ResultFilterModel> filterModels,
        ICollection<FilterOptionEntity> filterEntities, CancellationToken cancellationToken)
    {
        foreach (var filterModel in filterModels)
        {
            var filterOptionEntity = filterEntities
                .FirstOrDefault(x => x.FilterOptionId == filterModel.FilterOptionId);
            if (filterOptionEntity is null)
            {
                filterOptionEntity = CreateVersionEntity(user, new FilterOptionEntity());
                filterEntities.Add(filterOptionEntity);
                filterOptionEntity.LeagueId = leagueId;
            }
            await MapToFilterOptionEntityAsync(user, filterModel, filterOptionEntity, cancellationToken);
        }
        var deleteFilterEntities = filterEntities
            .Where(x => filterModels.Any(y => y.FilterOptionId == x.FilterOptionId) == false);
        foreach (var deleteFilterEntity in deleteFilterEntities)
        {
            filterEntities.Remove(deleteFilterEntity);
        }
        return filterEntities;
    }

    private Task<FilterOptionEntity> MapToFilterOptionEntityAsync(LeagueUser user, ResultFilterModel filterModel, FilterOptionEntity filterOptionEntity,
        CancellationToken cancellationToken)
    {
        var condition = filterOptionEntity.Conditions.FirstOrDefault();
        if (condition is null)
        {
            condition = new FilterConditionEntity();
            filterOptionEntity.Conditions.Add(condition);
        }
        condition.Comparator = filterModel.Comparator;
        condition.FilterType = filterModel.FilterType;
        condition.ColumnPropertyName = filterModel.ColumnPropertyName;
        condition.FilterValues = filterModel.FilterValues;
        condition.Action = filterModel.Action;
        UpdateVersionEntity(user, filterOptionEntity);
        return Task.FromResult(filterOptionEntity);
    }

    private async Task<ICollection<ScoringEntity>> MapToScoringList(long leagueId, LeagueUser user, ICollection<ScoringModel> scoringModels, 
        ICollection<ScoringEntity> scoringEntities, CancellationToken cancellationToken)
    {
        // Map votes
        foreach (var scoringModel in scoringModels)
        {
            var scoringEntity = scoringEntities
                .FirstOrDefault(x => x.ScoringId == scoringModel.Id);
            if (scoringEntity == null)
            {
                scoringEntity = CreateVersionEntity(user, new ScoringEntity());
                scoringEntities.Add(scoringEntity);
                scoringEntity.LeagueId = leagueId;
            }
            await MapToScoringEntityAsync(user, scoringModel, scoringEntity, cancellationToken);
        }
        // Delete votes that are no longer in source collection
        var deleteScorings = scoringEntities
            .Where(x => scoringModels.Any(y => y.Id == x.ScoringId) == false);
        foreach (var deleteScoring in deleteScorings)
        {
            dbContext.Remove(deleteScoring);
        }
        return scoringEntities;
    }

    private async Task<ScoringEntity> MapToScoringEntityAsync(LeagueUser user, ScoringModel scoringModel, ScoringEntity scoringEntity,
        CancellationToken cancellationToken)
    {
        scoringEntity.Index = scoringModel.Index;
        scoringEntity.MaxResultsPerGroup = scoringModel.MaxResultsPerGroup;
        scoringEntity.Name = scoringModel.Name;
        scoringEntity.ShowResults = scoringModel.ShowResults;
        scoringEntity.IsCombinedResult = scoringModel.IsCombinedResult;
        scoringEntity.UseResultSetTeam = scoringModel.UseResultSetTeam;
        scoringEntity.UpdateTeamOnRecalculation = scoringModel.UpdateTeamOnRecalculation;
        scoringEntity.PointsRule = scoringModel.PointRule is not null ? await MapToPointRuleEntityAsync(user, scoringModel.PointRule,
            scoringEntity.PointsRule ?? CreateVersionEntity(user, new PointRuleEntity() { LeagueId = scoringEntity.LeagueId }), cancellationToken) : null;
        UpdateVersionEntity(user, scoringEntity);
        return await Task.FromResult(scoringEntity);
    }

    private async Task<PointRuleEntity> MapToPointRuleEntityAsync(LeagueUser user, PointRuleModel pointRuleModel, PointRuleEntity pointRuleEntity,
        CancellationToken cancellationToken)
    {
        pointRuleEntity.BonusPoints = pointRuleModel.BonusPoints;
        pointRuleEntity.FinalSortOptions = pointRuleModel.FinalSortOptions;
        pointRuleEntity.MaxPoints = pointRuleModel.MaxPoints;
        pointRuleEntity.Name = pointRuleModel.Name;
        pointRuleEntity.PointDropOff = pointRuleModel.PointDropOff;
        pointRuleEntity.PointsPerPlace = pointRuleModel.PointsPerPlace;
        pointRuleEntity.PointsSortOptions = pointRuleModel.PointsSortOptions;
        UpdateVersionEntity(user, pointRuleEntity);
        return await Task.FromResult(pointRuleEntity);
    }

    protected virtual async Task<ResultConfigurationEntity> MapToResultConfigEntityAsync(LeagueUser user, PutResultConfigModel putResultConfig, ResultConfigurationEntity resultConfigEntity, CancellationToken cancellationToken)
    {
        return await MapToResultConfigEntityAsync(user, (PostResultConfigModel)putResultConfig, resultConfigEntity, cancellationToken);
    }

    protected virtual async Task<ResultConfigModel?> MapToResultConfigModel(long leagueId, long resultConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultConfigurations
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ResultConfigId == resultConfigId)
            .Select(MapToResultConfigModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Expression<Func<ResultConfigurationEntity, ResultConfigModel>> MapToResultConfigModelExpression => resultConfig => new ResultConfigModel()
    {
        LeagueId = resultConfig.LeagueId,
        ResultConfigId = resultConfig.ResultConfigId,
        SourceResultConfig = resultConfig.SourceResultConfig != null
            ? new ResultConfigInfoModel()
            {
                ResultConfigId = resultConfig.SourceResultConfig.ResultConfigId,
                DisplayName = resultConfig.SourceResultConfig.DisplayName,
                LeagueId = resultConfig.SourceResultConfig.LeagueId,
                Name = resultConfig.SourceResultConfig.Name,
            }
            : null,
        Name = resultConfig.Name,
        DisplayName = resultConfig.DisplayName,
        ResultKind = resultConfig.ResultKind,
        ResultsPerTeam = resultConfig.ResultsPerTeam,
        StandingConfig = resultConfig.StandingConfigurations.Select(standingConfig => new StandingConfigModel()
        {
            StandingConfigId = standingConfig.StandingConfigId,
            Name = standingConfig.Name,
            ResultKind = standingConfig.ResultKind,
            UseCombinedResult = standingConfig.UseCombinedResult,
            WeeksCounted = standingConfig.WeeksCounted,
        }).FirstOrDefault(),
        Scorings = resultConfig.Scorings.Select(scoring => new ScoringModel()
        {
            Id = scoring.ScoringId,
            MaxResultsPerGroup = scoring.MaxResultsPerGroup,
            Name = scoring.Name,
            ShowResults = scoring.ShowResults,
            IsCombinedResult = scoring.IsCombinedResult,
            UpdateTeamOnRecalculation = scoring.UpdateTeamOnRecalculation,
            UseResultSetTeam = scoring.UseResultSetTeam,
            PointRule = scoring.PointsRule != null ? new PointRuleModel()
            {
                BonusPoints = scoring.PointsRule.BonusPoints,
                FinalSortOptions = scoring.PointsRule.FinalSortOptions,
                LeagueId = scoring.LeagueId,
                MaxPoints = scoring.PointsRule.MaxPoints,
                PointDropOff = scoring.PointsRule.PointDropOff,
                PointRuleId = scoring.PointsRule.PointRuleId,
                PointsPerPlace = scoring.PointsRule.PointsPerPlace.ToList(),
                PointsSortOptions = scoring.PointsRule.PointsSortOptions,
                Name = scoring.PointsRule.Name,

            } : null,
        }).ToList(),
        FiltersForPoints = resultConfig.PointFilters.Select(filter => new ResultFilterModel()
        {
            LeagueId = filter.LeagueId,
            FilterOptionId = filter.FilterOptionId,
            FilterType = filter.Conditions.First().FilterType,
            FilterValues = filter.Conditions.First().FilterValues,
            Action = filter.Conditions.First().Action,
            ColumnPropertyName = filter.Conditions.First().ColumnPropertyName,
            Comparator = filter.Conditions.First().Comparator,
        }).ToList(),
        FiltersForResult = resultConfig.ResultFilters.Select(filter => new ResultFilterModel()
        {
            LeagueId = filter.LeagueId,
            FilterOptionId = filter.FilterOptionId,
            FilterType = filter.Conditions.First().FilterType,
            FilterValues = filter.Conditions.First().FilterValues,
            Action = filter.Conditions.First().Action,
            ColumnPropertyName = filter.Conditions.First().ColumnPropertyName,
            Comparator = filter.Conditions.First().Comparator,
        }).ToList(),
    };

    public Expression<Func<ScoringEntity, ScoringModel>> MapToGetScoringModelExpression => source => new ScoringModel()
    {
        Id = source.ScoringId,
        MaxResultsPerGroup = source.MaxResultsPerGroup,
        Name = source.Name,
        ShowResults = source.ShowResults,
        UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation,
        UseResultSetTeam = source.UseResultSetTeam,
    };
}
