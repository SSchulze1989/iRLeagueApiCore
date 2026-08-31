using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Models;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record GetStandingsFromEventRequest(long EventId) : IRequest<IEnumerable<StandingsModel>>;

public sealed class GetStandingsFromEventHandler : StandingsHandlerBase<GetStandingsFromEventHandler, GetStandingsFromEventRequest, IEnumerable<StandingsModel>>
{
    private readonly IMemoryCache memoryCache;

    public GetStandingsFromEventHandler(ILogger<GetStandingsFromEventHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetStandingsFromEventRequest>> validators, IMemoryCache memoryCache) : base(logger, dbContext, validators)
    {
        this.memoryCache = memoryCache;
    }

    public override async Task<IEnumerable<StandingsModel>> Handle(GetStandingsFromEventRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var cacheKey = CacheKeys.GetStandingsByEventKey(request.EventId);
        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<StandingsModel>? cached) && cached is not null)
        {
            return cached;
        }
        var getStandings = await MapToStandingModelFromEventAsync(request.EventId, cancellationToken);
        memoryCache.Set(cacheKey, getStandings, CacheKeys.StandingsCacheDuration);
        return getStandings;
    }

    private async Task<IEnumerable<StandingsModel>> MapToStandingModelFromEventAsync(long eventId, CancellationToken cancellationToken)
    {
        var standings = await dbContext.Standings
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.ChampSeason != null ? x.ChampSeason.Index : 999)
            .Select(MapToStandingModelExpression)
            .ToListAsync(cancellationToken);
        if (standings.Any() == false)
        {
            return standings;
        }
        standings = (await AlignStandingResultRows(standings.Select(x => x.SeasonId).First(), standings, cancellationToken)).ToList();
        return standings;
    }
}
