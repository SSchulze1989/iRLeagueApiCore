using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers;

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

    protected virtual async Task<LeagueEntity?> GetCurrentLeagueEntityAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Leagues
            .FirstOrDefaultAsync(x => x.Id == dbContext.LeagueProvider.LeagueId, cancellationToken);
    }

    protected virtual async Task<LeagueEntity?> GetLeagueEntityAsync(long leagueId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Leagues
            .FirstOrDefaultAsync(x => x.Id == leagueId, cancellationToken);
    }

    protected virtual async Task<ScheduleEntity?> GetScheduleEntityAsync(long? scheduleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Schedules
            .FirstOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken);
    }

    protected virtual async Task<ScoringEntity?> GetScoringEntityAsync(long? scoringId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Scorings
            .FirstOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
    }

    protected virtual async Task<SeasonEntity?> GetSeasonEntityAsync(long? seasonId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Seasons
            .FirstOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken);
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

    protected virtual async Task<VoteCategoryEntity?> GetVoteCategoryEntityAsync(long leagueId, long? voteCategoryId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VoteCategories
            .FirstOrDefaultAsync(x => x.CatId == voteCategoryId, cancellationToken);
    }

    protected virtual async Task<ICollection<MemberEntity>> GetMemberListAsync(IEnumerable<long> memberIds, CancellationToken cancellationToken = default)
    {
        return await dbContext.Members
            .Where(x => memberIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    protected virtual T CreateVersionEntity<T>(LeagueUser user, T target) where T : IVersionEntity
    {
        target.CreatedOn = DateTime.UtcNow;
        target.CreatedByUserId = user.Id;
        target.CreatedByUserName = user.Name;
        target.Version = 0;
        return target;
    }

    protected virtual T UpdateVersionEntity<T>(LeagueUser user, T target) where T : IVersionEntity
    {
        target.LastModifiedOn = DateTime.UtcNow;
        target.LastModifiedByUserId = user.Id;
        target.LastModifiedByUserName = user.Name;
        target.Version++;
        return target;
    }
}
