using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Seasons;

[Collection("DbTestFixture")]
public sealed class GetSeasonDbTestFixture : HandlersTestsBase<GetSeasonHandler, GetSeasonRequest, SeasonModel>
{
    public GetSeasonDbTestFixture() : base()
    {
    }

    protected override GetSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSeasonRequest> validator)
    {
        return new GetSeasonHandler(logger, dbContext, new IValidator<GetSeasonRequest>[] { validator });
    }

    protected override GetSeasonRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestSeasonId);
    }

    private GetSeasonRequest DefaultRequest(long leagueId, long seasonId)
    {
        return new GetSeasonRequest(leagueId, seasonId);
    }

    protected override void DefaultAssertions(GetSeasonRequest request, SeasonModel result, LeagueDbContext dbContext)
    {
        var testSeason = dbContext.Seasons
            .Include(x => x.Schedules)
            .First(x => x.SeasonId == request.SeasonId);
        result.LeagueId.Should().Be(request.LeagueId);
        result.SeasonId.Should().Be(request.SeasonId);
        result.Finished.Should().Be(testSeason.Finished);
        result.HideComments.Should().Be(testSeason.HideCommentsBeforeVoted);
        result.ScheduleIds.Should().BeEquivalentTo(testSeason.Schedules.Select(x => x.ScheduleId));
        result.SeasonEnd.Should().Be(testSeason.SeasonEnd);
        result.SeasonStart.Should().Be(testSeason.SeasonStart);
        result.SeasonName.Should().Be(testSeason.SeasonName);
        AssertVersion(testSeason, result);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public async override Task<SeasonModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 42)]
    [InlineData(42, 1)]
    public async Task HandleNotFound(long leagueId, long seasonId)
    {
        var request = DefaultRequest(leagueId, seasonId);
        await HandleNotFoundRequestAsync(request);
    }
}
