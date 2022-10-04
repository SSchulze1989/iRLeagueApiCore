using FluentValidation;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers
{
    public abstract class HandlerBase<THandler, TRequest>
    {
        protected ILogger<THandler> _logger;
        protected LeagueDbContext dbContext;
        protected IEnumerable<IValidator<TRequest>> validators;

        protected HandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.validators = validators;
        }

        protected virtual async Task<LeagueEntity?> GetLeagueEntityAsync(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Leagues
                .SingleOrDefaultAsync(x => x.Id == leagueId);
        }

        protected virtual async Task<ScheduleEntity?> GetScheduleEntityAsync(long leagueId, long? scheduleId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken);
        }

        protected virtual async Task<ScoringEntity?> GetScoringEntityAsync(long leagueId, long? scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
        }

        protected virtual async Task<SeasonEntity?> GetSeasonEntityAsync(long leagueId, long? seasonId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken);
        }

        protected virtual async Task<TrackConfigEntity?> GetTrackConfigEntityAsync(long? trackConfigId, CancellationToken cancellationToken = default)
        {
            return await dbContext.TrackConfigs
                .FirstOrDefaultAsync(x => x.TrackId == trackConfigId, cancellationToken);
        }

        protected virtual async Task<MemberEntity?> GetMemberEntityAsync(long? memberId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Members
                .FirstOrDefaultAsync(x => x.Id == memberId, cancellationToken);
        }

        protected virtual async Task<VoteCategoryEntity?> GetVoteCategoryEntityAsync(long leagueId, long? voteCategoryId)
        {
            return await dbContext.VoteCategories
                .FirstOrDefaultAsync(x => x.CatId == voteCategoryId);
        }

        protected virtual async Task<ICollection<MemberEntity>> GetMemberListAsync(IEnumerable<long> memberIds, CancellationToken cancellationToken = default)
        {
            return await dbContext.Members
                .Where(x => memberIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }

        protected virtual T CreateVersionEntity<T> (LeagueUser user, T target) where T : IVersionEntity
        {
            target.CreatedOn = DateTime.UtcNow;
            target.CreatedByUserId = user.Id;
            target.CreatedByUserName = user.Name;
            target.Version = 0;
            return target;
        }

        protected virtual T UpdateVersionEntity<T> (LeagueUser user, T target) where T : IVersionEntity
        {
            target.LastModifiedOn = DateTime.UtcNow;
            target.LastModifiedByUserId = user.Id;
            target.LastModifiedByUserName = user.Name;
            target.Version++;
            return target;
        }
    }
}
