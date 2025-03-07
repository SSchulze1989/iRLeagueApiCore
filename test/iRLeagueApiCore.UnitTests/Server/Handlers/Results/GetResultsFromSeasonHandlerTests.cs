﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

public sealed class GetResultsFromSeasonHandlerTests : ResultHandlersTestsBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest, IEnumerable<SeasonEventResultModel>>
{
    public GetResultsFromSeasonHandlerTests() : base()
    {
    }

    protected override GetResultsFromSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultsFromSeasonRequest> validator)
    {
        return new GetResultsFromSeasonHandler(logger, dbContext, new IValidator<GetResultsFromSeasonRequest>[] { validator });
    }

    protected override GetResultsFromSeasonRequest DefaultRequest()
    {
        return DefaultRequest(TestSeasonId);
    }

    private GetResultsFromSeasonRequest DefaultRequest(long seasonId)
    {
        return new GetResultsFromSeasonRequest(seasonId);
    }

    protected override void DefaultAssertions(GetResultsFromSeasonRequest request, IEnumerable<SeasonEventResultModel> result, LeagueDbContext dbContext)
    {
        base.DefaultAssertions(request, result, dbContext);
        var seasonResults = dbContext.ScoredEventResults
            .Where(x => x.Event.Schedule.SeasonId == request.SeasonId)
            .GroupBy(x => x.EventId);
        Assert.Equal(seasonResults.Count(), result.Count());
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
    public async Task HandleNotFoundAsync(long? leagueId, long? seasonId)
    {
        leagueId ??= TestLeagueId;
        seasonId ??= TestSeasonId;
        accessMockHelper.SetCurrentLeague(leagueId.Value);
        var request = DefaultRequest(seasonId.Value);
        await HandleNotFoundRequestAsync(request);
    }
}
