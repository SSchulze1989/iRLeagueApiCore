using FluentValidation;
using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Handlers.Standings;
using iRLeagueApiCore.Services.TriggerService.Events;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Standings;

public sealed class GetStandingsFromEventHandlerTests : StandingsHandlerTestsBase<GetStandingsFromEventHandler, GetStandingsFromEventRequest, IEnumerable<StandingsModel>>
{
    protected override GetStandingsFromEventHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetStandingsFromEventRequest> validator)
    {
        return new GetStandingsFromEventHandler(logger, dbContext, new[] { validator },
            new MemoryCache(new MemoryCacheOptions()));
    }

    protected override GetStandingsFromEventRequest DefaultRequest()
    {
        return new GetStandingsFromEventRequest(TestEventId);
    }

    [Fact]
    public async override Task ShouldHandleDefault()
    {
        await base.ShouldHandleDefault();
    }

    [Fact]
    public async override Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }

    [Fact]
    public async Task ShouldReturnCachedResult_OnSubsequentCall()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var handler = new GetStandingsFromEventHandler(logger, db,
            Array.Empty<IValidator<GetStandingsFromEventRequest>>(), sharedCache);
        var request = DefaultRequest();

        var firstResult = (await handler.Handle(request, default)).ToList();
        firstResult.Should().NotBeEmpty();

        // Wipe the DB — the second call must be served from cache
        db.Standings.RemoveRange(db.Standings);
        await db.SaveChangesAsync();

        var secondResult = (await handler.Handle(request, default)).ToList();
        secondResult.Should().BeEquivalentTo(firstResult);
    }

    [Fact]
    public async Task ShouldQueryDb_AfterCacheInvalidation()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var handler = new GetStandingsFromEventHandler(logger, db,
            Array.Empty<IValidator<GetStandingsFromEventRequest>>(), sharedCache);
        var request = DefaultRequest();

        // Populate the cache
        var cachedResult = (await handler.Handle(request, default)).ToList();
        cachedResult.Should().NotBeEmpty();

        // Invalidate
        var invalidationHandler = new StandingsCacheInvalidationHandler(sharedCache, db);
        await invalidationHandler.Handle(new StandingsUpdatedEventNotification(TestSeasonId), default);

        // Wipe the DB — the handler must now hit the DB and find nothing
        db.Standings.RemoveRange(db.Standings);
        await db.SaveChangesAsync();

        var result = (await handler.Handle(request, default)).ToList();
        result.Should().BeEmpty();
    }
}
