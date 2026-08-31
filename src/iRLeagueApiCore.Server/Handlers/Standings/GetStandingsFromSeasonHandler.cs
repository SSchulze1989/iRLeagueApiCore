using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Models;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record GetStandingsFromSeasonRequest(long SeasonId) : IRequest<IEnumerable<StandingsModel>>;

public sealed class GetStandingsFromSeasonHandler : StandingsHandlerBase<GetStandingsFromSeasonHandler, GetStandingsFromSeasonRequest, IEnumerable<StandingsModel>>
{
    private readonly IMemoryCache memoryCache;

    public GetStandingsFromSeasonHandler(ILogger<GetStandingsFromSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetStandingsFromSeasonRequest>> validators, IMemoryCache memoryCache) : base(logger, dbContext, validators)
    {
        this.memoryCache = memoryCache;
    }

    public override async Task<IEnumerable<StandingsModel>> Handle(GetStandingsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var cacheKey = CacheKeys.GetStandingsBySeasonKey(request.SeasonId);
        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<StandingsModel>? cached) && cached is not null)
        {
            return cached;
        }
        var getStandings = await MapToStandingModelFromSeasonAsync(request.SeasonId, cancellationToken);
        memoryCache.Set(cacheKey, getStandings, CacheKeys.StandingsCacheDuration);
        return getStandings;
    }

    private async Task<IEnumerable<StandingsModel>> MapToStandingModelFromSeasonAsync(long seasonId, CancellationToken cancellationToken)
    {
        var lastEventWithStandingsId = await dbContext.Standings
            .Where(x => x.SeasonId == seasonId)
            .OrderByDescending(x => x.Event.Date)
            .Select(x => x.EventId)
            .FirstOrDefaultAsync(cancellationToken);
        var standings = await dbContext.Standings
            .Where(x => x.EventId == lastEventWithStandingsId)
            .OrderBy(x => x.ChampSeason != null ? x.ChampSeason.Index : 999)
            .Select(MapToStandingModelExpression)
            .ToListAsync(cancellationToken);
        if (standings.Any() == false)
        {
            return standings;
        }
        standings = (await AlignStandingResultRows(seasonId, standings, cancellationToken)).ToList();
        return standings;
    }
}
