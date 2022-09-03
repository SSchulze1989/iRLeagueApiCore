using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public class ResultConfigHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public ResultConfigHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<ResultConfigurationEntity> MapToResultConfigEntityAsync(LeagueUser user, PostResultConfigModel postResultConfig, 
            ResultConfigurationEntity resultConfigEntity, CancellationToken cancellationToken)
        {
            resultConfigEntity.DisplayName = postResultConfig.DisplayName;
            resultConfigEntity.Name = postResultConfig.Name;
            UpdateVersionEntity(user, resultConfigEntity);
            return await Task.FromResult(resultConfigEntity);
        }

        protected virtual async Task<ResultConfigModel> MapToResultConfigModel(long leagueId, long resultConfigId, CancellationToken cancellationToken)
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
            Name = resultConfig.Name,
            DisplayName = resultConfig.DisplayName,
            Scorings = resultConfig.Scorings.Select(scoring => new ScoringModel()
            {
                Id = scoring.ScoringId,
                MaxResultsPerGroup = scoring.MaxResultsPerGroup,
                Name = scoring.Name,
                ShowResults = scoring.ShowResults,
                UpdateTeamOnRecalculation = scoring.UpdateTeamOnRecalculation,
                UseResultSetTeam = scoring.UseResultSetTeam,
            }),
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
