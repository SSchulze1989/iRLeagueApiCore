using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.TriggerService.Events;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public sealed class StandingsCacheInvalidationHandler : INotificationHandler<StandingsUpdatedEventNotification>
{
    private readonly IMemoryCache memoryCache;
    private readonly LeagueDbContext dbContext;

    public StandingsCacheInvalidationHandler(IMemoryCache memoryCache, LeagueDbContext dbContext)
    {
        this.memoryCache = memoryCache;
        this.dbContext = dbContext;
    }

    public async Task Handle(StandingsUpdatedEventNotification notification, CancellationToken cancellationToken)
    {
        var seasonId = notification.seasonId;

        memoryCache.Remove(CacheKeys.GetStandingsBySeasonKey(seasonId));

        var eventIds = await dbContext.Events
            .Where(x => x.Schedule.SeasonId == seasonId)
            .Select(x => x.EventId)
            .ToListAsync(cancellationToken);

        foreach (var eventId in eventIds)
        {
            memoryCache.Remove(CacheKeys.GetStandingsByEventKey(eventId));
        }
    }
}
