using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Services.TriggerService.Events;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

public sealed class GetResultsFromSessionHandlerTests : ResultHandlersTestsBase<GetResultsFromSessionHandler, GetResultsFromEventRequest, IEnumerable<EventResultModel>>
{
    protected override GetResultsFromSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultsFromEventRequest> validator)
    {
        return new GetResultsFromSessionHandler(logger, dbContext, new[] { validator },
            new MemoryCache(new MemoryCacheOptions()));
    }

    protected override GetResultsFromEventRequest DefaultRequest()
    {
        return new GetResultsFromEventRequest(TestEventId);
    }

    protected override void DefaultAssertions(GetResultsFromEventRequest request, IEnumerable<EventResultModel> result, LeagueDbContext dbContext)
    {
        base.DefaultAssertions(request, result, dbContext);
        var eventResults = dbContext.ScoredEventResults
            .Where(x => x.EventId == request.EventId);
        result.Should().HaveSameCount(eventResults);
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

    [Theory]
    [InlineData(0L, defaultId)]
    [InlineData(defaultId, 0L)]
    [InlineData(-42L, defaultId)]
    [InlineData(defaultId, -42L)]
    public async Task HandleNotFoundAsync(long? leagueId, long? eventId)
    {
        leagueId ??= TestLeagueId;
        eventId ??= TestEventId;
        accessMockHelper.SetCurrentLeague(leagueId.Value);
        var request = new GetResultsFromEventRequest(eventId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public async Task ShouldReturnCachedResult_OnSubsequentCall()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var handler = new GetResultsFromSessionHandler(logger, db,
            Array.Empty<IValidator<GetResultsFromEventRequest>>(), sharedCache);
        var request = DefaultRequest();

        var firstResult = await handler.Handle(request, default);

        // Wipe the DB — the second call must be served from cache
        db.ScoredEventResults.RemoveRange(db.ScoredEventResults);
        await db.SaveChangesAsync();

        var secondResult = await handler.Handle(request, default);

        secondResult.Should().BeEquivalentTo(firstResult);
    }

    [Fact]
    public async Task ShouldQueryDb_AfterCacheInvalidation()
    {
        var sharedCache = new MemoryCache(new MemoryCacheOptions());
        using var db = accessMockHelper.CreateMockDbContext(databaseName);
        var handler = new GetResultsFromSessionHandler(logger, db,
            Array.Empty<IValidator<GetResultsFromEventRequest>>(), sharedCache);
        var request = DefaultRequest();

        // Populate the cache
        await handler.Handle(request, default);

        // Invalidate
        var invalidationHandler = new ResultCacheInvalidationHandler(sharedCache, db);
        await invalidationHandler.Handle(new ResultCalculatedEventNotification(TestEventId), default);

        // Wipe the DB — the handler must now hit the DB and find nothing
        db.ScoredEventResults.RemoveRange(db.ScoredEventResults);
        await db.SaveChangesAsync();

        var act = async () => await handler.Handle(request, default);
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }
}
