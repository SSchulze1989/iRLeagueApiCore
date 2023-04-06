using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Leagues;

public class LeagueHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>

{
    public LeagueHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    protected virtual LeagueEntity MapToLeagueEntity(LeagueUser user, PostLeagueModel postLeague, LeagueEntity leagueEntity)
    {
        leagueEntity.Name = postLeague.Name;
        leagueEntity.NameFull = postLeague.NameFull;
        leagueEntity.LeaguePublic = postLeague.LeaguePublic;
        leagueEntity.Description = postLeague.Description;
        leagueEntity.DescriptionPlain = postLeague.DescriptionPlain;
        CreateVersionEntity(user, leagueEntity);
        UpdateVersionEntity(user, leagueEntity);
        return leagueEntity;
    }

    protected virtual async Task<LeagueModel?> MapToGetLeagueModelAsync(long leagueId, CancellationToken cancellationToken)
    {
        return await dbContext.Leagues
            .Where(x => x.Id == leagueId)
            .Select(MapToGetLeagueModelExpression)
            .SingleOrDefaultAsync(cancellationToken);
    }

    protected Expression<Func<LeagueEntity, LeagueModel>> MapToGetLeagueModelExpression => x => new LeagueModel()
    {
        Id = x.Id,
        Name = x.Name,
        NameFull = x.NameFull,
        Description = x.Description,
        DescriptionPlain = x.DescriptionPlain,
        EnableProtests = x.EnableProtests,
        ProtestCoolDownPeriod = x.ProtestCoolDownPeriod,
        ProtestsClosedAfter = x.ProtestsClosedAfter,
        ProtestsPublic = x.ProtestsPublic,
        SeasonIds = x.Seasons
            .Select(season => season.SeasonId)
            .ToList(),
        CreatedByUserId = x.CreatedByUserId,
        CreatedByUserName = x.CreatedByUserName,
        CreatedOn = x.CreatedOn,
        LastModifiedByUserId = x.LastModifiedByUserId,
        LastModifiedByUserName = x.LastModifiedByUserName,
        LastModifiedOn = x.LastModifiedOn,
    };

    protected virtual LeagueEntity MapToLeagueEntity(long leagueId, LeagueUser user, PutLeagueModel putLeague, LeagueEntity leagueEntity)
    {
        leagueEntity.NameFull = putLeague.NameFull;
        leagueEntity.EnableProtests = putLeague.EnableProtests;
        leagueEntity.ProtestCoolDownPeriod = putLeague.ProtestCoolDownPeriod;
        leagueEntity.ProtestsClosedAfter = putLeague.ProtestsClosedAfter;
        leagueEntity.ProtestsPublic = putLeague.ProtestsPublic;
        leagueEntity.LeaguePublic = putLeague.LeaguePublic;
        UpdateVersionEntity(user, leagueEntity);
        return leagueEntity;
    }
}
