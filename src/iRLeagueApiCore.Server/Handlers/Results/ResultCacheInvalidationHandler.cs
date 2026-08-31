using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.TriggerService.Events;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Handlers.Results;

public sealed class ResultCacheInvalidationHandler : INotificationHandler<ResultCalculatedEventNotification>
{
    private readonly IMemoryCache memoryCache;
    private readonly LeagueDbContext dbContext;

    public ResultCacheInvalidationHandler(IMemoryCache memoryCache, LeagueDbContext dbContext)
    {
        this.memoryCache = memoryCache;
        this.dbContext = dbContext;
    }

    public async Task Handle(ResultCalculatedEventNotification notification, CancellationToken cancellationToken)
    {
        var eventId = notification.EventId;

        // Invalidate result cache for this event
        memoryCache.Remove(CacheKeys.GetResultsByEventKey(eventId));

        // Look up the season to invalidate season-level result and standings caches
        var seasonId = await dbContext.Events
            .Where(x => x.EventId == eventId)
            .Select(x => x.Schedule.SeasonId)
            .FirstOrDefaultAsync(cancellationToken);

        if (seasonId != default)
        {
            memoryCache.Remove(CacheKeys.GetResultsBySeasonKey(seasonId));

            // Standings for all events in the season may be recalculated, clear them all
            var eventIds = await dbContext.Events
                .Where(x => x.Schedule.SeasonId == seasonId)
                .Select(x => x.EventId)
                .ToListAsync(cancellationToken);

            foreach (var id in eventIds)
            {
                memoryCache.Remove(CacheKeys.GetStandingsByEventKey(id));
            }

            memoryCache.Remove(CacheKeys.GetStandingsBySeasonKey(seasonId));
        }
    }
}
