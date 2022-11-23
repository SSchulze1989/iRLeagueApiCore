using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public class PointRuleHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public PointRuleHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<PointRuleEntity?> GetPointRuleEntityAsync(long leagueId, long pointRuleId, CancellationToken cancellationToken)
        {
            return await dbContext.PointRules
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.PointRuleId == pointRuleId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        protected virtual async Task<PointRuleEntity> MapToPointRuleEntityAsync(LeagueUser user, PostPointRuleModel postPointRule, PointRuleEntity pointRuleEntity, 
            CancellationToken cancellationToken)
        {
            pointRuleEntity.BonusPoints = postPointRule.BonusPoints;
            pointRuleEntity.FinalSortOptions = postPointRule.FinalSortOptions.ToList();
            pointRuleEntity.MaxPoints = postPointRule.MaxPoints;
            pointRuleEntity.Name = postPointRule.Name;
            pointRuleEntity.PointDropOff = postPointRule.PointDropOff;
            pointRuleEntity.PointsPerPlace = postPointRule.PointsPerPlace.ToList();
            pointRuleEntity.PointsSortOptions = postPointRule.PointsSortOptions.ToList();
            UpdateVersionEntity(user, pointRuleEntity);
            return await Task.FromResult(pointRuleEntity);
        }

        protected virtual async Task<PointRuleModel?> MapToPointRuleModel(long leagueId, long pointRuleId, CancellationToken cancellationToken)
        {
            return await dbContext.PointRules
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.PointRuleId == pointRuleId)
                .Select(MapToPointRuleModelExpression)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private Expression<Func<PointRuleEntity, PointRuleModel>> MapToPointRuleModelExpression => pointRule => new PointRuleModel()
        {
            BonusPoints = pointRule.BonusPoints,
            FinalSortOptions = pointRule.FinalSortOptions,
            LeagueId = pointRule.LeagueId,
            MaxPoints = pointRule.MaxPoints,
            Name = pointRule.Name,
            PointDropOff = pointRule.PointDropOff,
            PointRuleId = pointRule.PointRuleId,
            PointsPerPlace = pointRule.PointsPerPlace.ToList(),
            PointsSortOptions = pointRule.PointsSortOptions,
        };
    }
}
