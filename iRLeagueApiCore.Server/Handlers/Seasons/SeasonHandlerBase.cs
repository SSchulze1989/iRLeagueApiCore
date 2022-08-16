using FluentValidation;
using iRLeagueApiCore.Common.Models;
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

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public abstract class SeasonHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        protected SeasonHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<SeasonEntity> MapToSeasonEntityAsync(long leagueId, LeagueUser user, PutSeasonModel putSeason, 
            SeasonEntity target, CancellationToken cancellationToken)
        {
            target.Finished = putSeason.Finished;
            target.HideCommentsBeforeVoted = putSeason.HideComments;
            target.MainScoring = await GetScoringEntityAsync(leagueId, putSeason.MainScoringId, cancellationToken);
            target.SeasonName = putSeason.SeasonName;
            target.LastModifiedOn = DateTime.Now;
            target.LastModifiedByUserId = user.Id;
            target.LastModifiedByUserName = user.Name;
            target.Version++;
            return target;
        }

        protected virtual async Task<SeasonModel> MapToGetSeasonModel(long leagueId, long seasonId, CancellationToken cancellationToken)
        {
            return await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SeasonId == seasonId)
                .Select(MapToGetSeasonModelExpression)
                .SingleOrDefaultAsync();
        }

        protected Expression<Func<SeasonEntity, SeasonModel>> MapToGetSeasonModelExpression => x => new SeasonModel()
        {
            SeasonId = x.SeasonId,
            Finished = x.Finished,
            HideComments = x.HideCommentsBeforeVoted,
            LeagueId = x.LeagueId,
            MainScoringId = x.MainScoringScoringId,
            ScheduleIds = x.Schedules.Select(x => x.ScheduleId).ToList(),
            SeasonEnd = x.Schedules
                .SelectMany(x => x.Sessions)
                .Select(x => x.Date)
                .OrderByDescending(x => x)
                .FirstOrDefault(),
            SeasonStart = x.Schedules
                .SelectMany(x => x.Sessions)
                .Select(x => x.Date)
                .OrderBy(x => x)
                .FirstOrDefault(),
            SeasonName = x.SeasonName,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByUserName = x.CreatedByUserName,
            CreatedOn = x.CreatedOn,
            LastModifiedByUserId = x.LastModifiedByUserId,
            LastModifiedByUserName = x.LastModifiedByUserName,
            LastModifiedOn = x.LastModifiedOn,
        };
    }
}
