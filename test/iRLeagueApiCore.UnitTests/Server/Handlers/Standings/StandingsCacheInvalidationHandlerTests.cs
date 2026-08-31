using FluentValidation;
using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Handlers.Standings;
using iRLeagueApiCore.Services.TriggerService.Events;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Standings;

/// <summary>
/// Tests for <see cref="StandingsCacheInvalidationHandler"/>.
/// Verifies that <see cref="StandingsUpdatedEventNotification"/> clears the standings
/// caches by event and by season for the affected season.
/// </summary>
public sealed class StandingsCacheInvalidationHandlerTests : StandingsHandlerTestsBase<
    GetStandingsFromEventHandler,
    GetStandingsFromEventRequest,
    IEnumerable<StandingsModel>>
{
    protected override GetStandingsFromEventHandler CreateTestHandler(
        LeagueDbContext dbContext, IValidator<GetStandingsFromEventRequest> validator)
    {
        return new GetStandingsFromEventHandler(logger, dbContext, new[] { validator },
            new MemoryCache(new MemoryCacheOptions()));
    }

    protected override GetStandingsFromEventRequest DefaultRequest() =>
        new(TestEventId);

    // ── standings-by-event cache ─────────────────────────────────────────────

    [Fact]
    public async Task ShouldInvalidateStandingsByEventCache_AfterStandingsUpdated()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var handler = new GetStandingsFromEventHandler(logger, db,
            Array.Empty<IValidator<GetStandingsFromEventRequest>>(), sharedCache);

        // Populate cache
        var cached = (await handler.Handle(new GetStandingsFromEventRequest(TestEventId), default)).ToList();
        cached.Should().NotBeEmpty();

        // Invalidate
        var sut = new StandingsCacheInvalidationHandler(sharedCache, db);
        await sut.Handle(new StandingsUpdatedEventNotification(TestSeasonId), default);

        // Cache is gone — wipe DB and verify the handler now returns empty
        db.Standings.RemoveRange(db.Standings);
        await db.SaveChangesAsync();

        var result = (await handler.Handle(new GetStandingsFromEventRequest(TestEventId), default)).ToList();
        result.Should().BeEmpty();
    }

    // ── standings-by-season cache ────────────────────────────────────────────

    [Fact]
    public async Task ShouldInvalidateStandingsBySeasonCache_AfterStandingsUpdated()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var seasonLogger = Mock.Of<ILogger<GetStandingsFromSeasonHandler>>();
        var handler = new GetStandingsFromSeasonHandler(seasonLogger, db,
            Array.Empty<IValidator<GetStandingsFromSeasonRequest>>(), sharedCache);

        // Populate cache
        var cached = (await handler.Handle(new GetStandingsFromSeasonRequest(TestSeasonId), default)).ToList();
        cached.Should().NotBeEmpty();

        // Invalidate
        var sut = new StandingsCacheInvalidationHandler(sharedCache, db);
        await sut.Handle(new StandingsUpdatedEventNotification(TestSeasonId), default);

        // Cache is gone — wipe DB and verify the handler now returns empty
        db.Standings.RemoveRange(db.Standings);
        await db.SaveChangesAsync();

        var result = (await handler.Handle(new GetStandingsFromSeasonRequest(TestSeasonId), default)).ToList();
        result.Should().BeEmpty();
    }
}
