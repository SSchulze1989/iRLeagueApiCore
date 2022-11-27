﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results
{
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
            resultConfigEntity.Scorings = await MapToScoringList(resultConfigEntity.LeagueId, user, postResultConfig.Scorings, resultConfigEntity.Scorings, cancellationToken);
            UpdateVersionEntity(user, resultConfigEntity);
            return await Task.FromResult(resultConfigEntity);
        }

        private async Task<ICollection<ScoringEntity>> MapToScoringList(long leagueId, LeagueUser user, ICollection<ScoringModel> scoringModels, ICollection<ScoringEntity> scoringEntities, 
            CancellationToken cancellationToken)
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
            Scorings = resultConfig.Scorings.Select(scoring => new ScoringModel()
            {
                Id = scoring.ScoringId,
                MaxResultsPerGroup = scoring.MaxResultsPerGroup,
                Name = scoring.Name,
                ShowResults = scoring.ShowResults,
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
}
