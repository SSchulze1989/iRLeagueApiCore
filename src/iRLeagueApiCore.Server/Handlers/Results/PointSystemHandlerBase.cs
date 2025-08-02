using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public abstract class PointSystemHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public PointSystemHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<PointSystemEntity?> GetResultConfigEntity(long resultConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultConfigurations
            .Include(x => x.ChampSeason)
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
                    .ThenInclude(x => x.AutoPenalties)
            .Include(x => x.SourcePointSystem)
            .Include(x => x.ResultFilters)
            .Include(x => x.PointFilters)
            .Where(x => x.PointSystemId == resultConfigId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual async Task<PointSystemEntity> MapToResultConfigEntityAsync(LeagueUser user, PostPointSystemModel postResultConfig,
        PointSystemEntity resultConfigEntity, CancellationToken cancellationToken)
    {
        resultConfigEntity.DisplayName = postResultConfig.DisplayName;
        resultConfigEntity.SourcePointSystem = postResultConfig.SourceResultConfig != null
            ? await dbContext.ResultConfigurations
                .FirstOrDefaultAsync(x => x.PointSystemId == postResultConfig.SourceResultConfig.ResultConfigId, cancellationToken)
            : null;
        resultConfigEntity.Name = postResultConfig.Name;
        resultConfigEntity.ResultsPerTeam = postResultConfig.ResultsPerTeam;
        resultConfigEntity.Scorings = await MapToScoringList(user, postResultConfig.Scorings, resultConfigEntity.Scorings, cancellationToken);
        resultConfigEntity.PointFilters = await MapToFilterOptionListAsync(user, postResultConfig.FiltersForPoints,
            resultConfigEntity.PointFilters, cancellationToken);
        resultConfigEntity.ResultFilters = await MapToFilterOptionListAsync(user, postResultConfig.FiltersForResult,
            resultConfigEntity.ResultFilters, cancellationToken);
        UpdateVersionEntity(user, resultConfigEntity);
        return await Task.FromResult(resultConfigEntity);
    }

    private async Task<ICollection<StandingConfigurationEntity>> MapToStandingConfigListAsync(LeagueUser user, StandingConfigModel? standingConfigModel,
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
            standingConfigEntity.LeagueId = dbContext.LeagueProvider.LeagueId;
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

    private async Task<ICollection<ScoringEntity>> MapToScoringList(LeagueUser user, ICollection<ScoringModel> scoringModels,
        ICollection<ScoringEntity> scoringEntities, CancellationToken cancellationToken)
    {
        // Map votes
        foreach (var scoringModel in scoringModels)
        {
            var scoringEntity = scoringModel.Id == 0 ? null : scoringEntities
                .FirstOrDefault(x => x.ScoringId == scoringModel.Id);
            if (scoringEntity == null)
            {
                scoringEntity = CreateVersionEntity(user, new ScoringEntity());
                scoringEntities.Add(scoringEntity);
                scoringEntity.LeagueId = dbContext.LeagueProvider.LeagueId;
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
            scoringEntity.PointsRule ?? CreateVersionEntity(user, new PointRuleEntity() { LeagueId = dbContext.LeagueProvider.LeagueId }), cancellationToken) : null;
        scoringEntity.UseExternalSourcePoints = scoringModel.UseSourcePoints;
        UpdateVersionEntity(user, scoringEntity);
        return await Task.FromResult(scoringEntity);
    }

    private async Task<PointRuleEntity> MapToPointRuleEntityAsync(LeagueUser user, PointRuleModel pointRuleModel, PointRuleEntity pointRuleEntity,
        CancellationToken cancellationToken)
    {
        pointRuleEntity.RuleType = pointRuleModel.RuleType;
        pointRuleEntity.BonusPoints = pointRuleModel.BonusPoints;
        pointRuleEntity.FinalSortOptions = pointRuleModel.FinalSortOptions;
        pointRuleEntity.MaxPoints = pointRuleModel.MaxPoints;
        pointRuleEntity.Name = pointRuleModel.Name;
        pointRuleEntity.PointDropOff = pointRuleModel.PointDropOff;
        pointRuleEntity.PointsPerPlace = pointRuleModel.PointsPerPlace;
        pointRuleEntity.PointsSortOptions = pointRuleModel.PointsSortOptions;
        pointRuleEntity.Formula = pointRuleModel.Formula;
        pointRuleEntity.AutoPenalties = await MapToAutoPenaltyCollection(pointRuleModel.AutoPenalties, pointRuleEntity.AutoPenalties, cancellationToken);
        UpdateVersionEntity(user, pointRuleEntity);
        return pointRuleEntity;
    }

    private async Task<ICollection<AutoPenaltyConfigEntity>> MapToAutoPenaltyCollection(IEnumerable<AutoPenaltyConfiguration> models, ICollection<AutoPenaltyConfigEntity> entities,
        CancellationToken cancellationToken)
    {
        foreach (var model in models)
        {
            var entity = entities
                .Where(x => x.PenaltyConfigId != 0 && x.PenaltyConfigId == model.PenaltyConfigId)
                .FirstOrDefault();
            if (entity is null)
            {
                entity = new() { LeagueId = dbContext.LeagueProvider.LeagueId };
                entities.Add(entity);
            }
            await MapToAutoPenaltyConfigEntity(model, entity, cancellationToken);
        }
        var delete = entities
            .Where(x => models.Any(y => y.PenaltyConfigId == x.PenaltyConfigId) == false);
        foreach (var deleteEntity in delete)
        {
            dbContext.Remove(deleteEntity);
        }
        return entities;
    }

    private async Task<AutoPenaltyConfigEntity> MapToAutoPenaltyConfigEntity(AutoPenaltyConfiguration model, AutoPenaltyConfigEntity entity,
        CancellationToken cancellationToken)
    {
        entity.Conditions = model.Conditions;
        entity.Description = model.Description;
        entity.Points = model.Points;
        entity.Positions = model.Positions;
        entity.Time = model.Time;
        entity.Type = model.Type;
        return await Task.FromResult(entity);
    }

    protected virtual async Task<PointSystemEntity> MapToResultConfigEntityAsync(LeagueUser user, PutPointSystemModel putResultConfig, PointSystemEntity resultConfigEntity, CancellationToken cancellationToken)
    {
        return await MapToResultConfigEntityAsync(user, (PostPointSystemModel)putResultConfig, resultConfigEntity, cancellationToken);
    }

    protected virtual async Task<PointSystemModel?> MapToResultConfigModel(long resultConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultConfigurations
            .Where(x => x.PointSystemId == resultConfigId)
            .Select(MapToResultConfigModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Expression<Func<PointSystemEntity, PointSystemModel>> MapToResultConfigModelExpression => resultConfig => new PointSystemModel()
    {
        LeagueId = resultConfig.LeagueId,
        PointSystemId = resultConfig.PointSystemId,
        ChampSeasonId = resultConfig.ChampSeasonId,
        ChampionshipName = resultConfig.ChampSeason.Championship.Name,
        SourceResultConfig = resultConfig.SourcePointSystem != null
            ? new PointSystemInfoModel()
            {
                ResultConfigId = resultConfig.SourcePointSystem.PointSystemId,
                ChampSeasonId = resultConfig.SourcePointSystem.ChampSeasonId,
                ChampionshipName = resultConfig.SourcePointSystem.ChampSeason.Championship.Name,
                DisplayName = resultConfig.SourcePointSystem.DisplayName,
                LeagueId = resultConfig.SourcePointSystem.LeagueId,
                Name = resultConfig.SourcePointSystem.Name,
            }
            : null,
        Name = resultConfig.Name,
        DisplayName = resultConfig.DisplayName,
        IsDefaultConfig = resultConfig.PointSystemId == resultConfig.ChampSeason.DefaultPointSystemId,
        ResultsPerTeam = resultConfig.ResultsPerTeam,
        Scorings = resultConfig.Scorings.Select(scoring => new ScoringModel()
        {
            Id = scoring.ScoringId,
            Index = scoring.Index,
            MaxResultsPerGroup = scoring.MaxResultsPerGroup,
            Name = scoring.Name,
            ShowResults = scoring.ShowResults,
            IsCombinedResult = scoring.IsCombinedResult,
            UpdateTeamOnRecalculation = scoring.UpdateTeamOnRecalculation,
            UseResultSetTeam = scoring.UseResultSetTeam,
            UseSourcePoints = scoring.UseExternalSourcePoints,
            PointRule = scoring.PointsRule != null ? new PointRuleModel()
            {
                RuleType = scoring.PointsRule.RuleType,
                BonusPoints = scoring.PointsRule.BonusPoints,
                FinalSortOptions = scoring.PointsRule.FinalSortOptions,
                LeagueId = scoring.LeagueId,
                MaxPoints = scoring.PointsRule.MaxPoints,
                PointDropOff = scoring.PointsRule.PointDropOff,
                PointRuleId = scoring.PointsRule.PointRuleId,
                PointsPerPlace = scoring.PointsRule.PointsPerPlace.ToList(),
                PointsSortOptions = scoring.PointsRule.PointsSortOptions,
                Name = scoring.PointsRule.Name,
                Formula = scoring.PointsRule.Formula,
                AutoPenalties = scoring.PointsRule.AutoPenalties.Select(penalty => new AutoPenaltyConfiguration()
                {
                    Conditions = penalty.Conditions,
                    Description = penalty.Description,
                    LeagueId = penalty.LeagueId,
                    PenaltyConfigId = penalty.PenaltyConfigId,
                    Points = penalty.Points,
                    Positions = penalty.Positions,
                    Time = penalty.Time,
                    Type = penalty.Type,
                }).ToList(),
            } : null,
        }).OrderBy(x => x.Index).ToList(),
        FiltersForPoints = resultConfig.PointFilters
            .Select(filter => new ResultFilterModel()
            {
                LeagueId = filter.LeagueId,
                FilterOptionId = filter.FilterOptionId,
                Condition = filter.Conditions.FirstOrDefault() ?? new(),
            }).ToList(),
        FiltersForResult = resultConfig.ResultFilters
            .Select(filter => new ResultFilterModel()
            {
                LeagueId = filter.LeagueId,
                FilterOptionId = filter.FilterOptionId,
                Condition = filter.Conditions.FirstOrDefault() ?? new(),
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
