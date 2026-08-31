using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Server.Handlers.Standings;
using iRLeagueApiCore.Services.TriggerService.Events;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

/// <summary>
/// Tests for <see cref="ResultCacheInvalidationHandler"/>.
/// Verifies that <see cref="ResultCalculatedEventNotification"/> clears the result
/// caches (by event and by season) and the standings caches for all events in the season.
/// </summary>
public sealed class ResultCacheInvalidationHandlerTests : ResultHandlersTestsBase<
    GetResultsFromSessionHandler,
    GetResultsFromEventRequest,
    IEnumerable<EventResultModel>>
{
    protected override GetResultsFromSessionHandler CreateTestHandler(
        LeagueDbContext dbContext, IValidator<GetResultsFromEventRequest> validator)
    {
        return new GetResultsFromSessionHandler(logger, dbContext, new[] { validator },
            new MemoryCache(new MemoryCacheOptions()));
    }

    protected override GetResultsFromEventRequest DefaultRequest() =>
        new(TestEventId);

    // ── result-by-event cache ────────────────────────────────────────────────

    [Fact]
    public async Task ShouldInvalidateResultByEventCache_AfterResultCalculated()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var getHandler = new GetResultsFromSessionHandler(logger, db,
            Array.Empty<IValidator<GetResultsFromEventRequest>>(), sharedCache);

        await getHandler.Handle(new GetResultsFromEventRequest(TestEventId), default);

        var sut = new ResultCacheInvalidationHandler(sharedCache, db);
        await sut.Handle(new ResultCalculatedEventNotification(TestEventId), default);

        db.ScoredEventResults.RemoveRange(db.ScoredEventResults);
        await db.SaveChangesAsync();

        var act = async () => await getHandler.Handle(new GetResultsFromEventRequest(TestEventId), default);
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    // ── result-by-season cache ───────────────────────────────────────────────

    [Fact]
    public async Task ShouldInvalidateResultBySeasonCache_AfterResultCalculated()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var seasonLogger = Mock.Of<ILogger<GetResultsFromSeasonHandler>>();
        var getHandler = new GetResultsFromSeasonHandler(seasonLogger, db,
            Array.Empty<IValidator<GetResultsFromSeasonRequest>>(), sharedCache);

        await getHandler.Handle(new GetResultsFromSeasonRequest(TestSeasonId), default);

        var sut = new ResultCacheInvalidationHandler(sharedCache, db);
        await sut.Handle(new ResultCalculatedEventNotification(TestEventId), default);

        db.ScoredEventResults.RemoveRange(db.ScoredEventResults);
        await db.SaveChangesAsync();

        var act = async () => await getHandler.Handle(new GetResultsFromSeasonRequest(TestSeasonId), default);
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    // ── standings-by-event cache ─────────────────────────────────────────────

    [Fact]
    public async Task ShouldInvalidateStandingsByEventCache_AfterResultCalculated()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);

        var standing = new StandingEntity
        {
            LeagueId = TestLeagueId,
            SeasonId = TestSeasonId,
            EventId = TestEventId,
            Name = "Test Standing",
            StandingRows = new HashSet<StandingRowEntity>()
        };
        db.Standings.Add(standing);
        await db.SaveChangesAsync();

        var standingsLogger = Mock.Of<ILogger<GetStandingsFromEventHandler>>();
        var standingsHandler = new GetStandingsFromEventHandler(standingsLogger, db,
            Array.Empty<IValidator<GetStandingsFromEventRequest>>(), sharedCache);

        var cached = (await standingsHandler.Handle(new GetStandingsFromEventRequest(TestEventId), default)).ToList();
        cached.Should().NotBeEmpty();

        var sut = new ResultCacheInvalidationHandler(sharedCache, db);
        await sut.Handle(new ResultCalculatedEventNotification(TestEventId), default);

        db.Standings.RemoveRange(db.Standings);
        await db.SaveChangesAsync();

        var result = (await standingsHandler.Handle(new GetStandingsFromEventRequest(TestEventId), default)).ToList();
        result.Should().BeEmpty();
    }
}
