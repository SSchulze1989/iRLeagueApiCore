using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Schedules;

public sealed class PostScheduleDbTestFixture : HandlersTestsBase<PostScheduleHandler, PostScheduleRequest, ScheduleModel>
{
    private const string testScheduleName = "TestSchedule";

    public PostScheduleDbTestFixture() : base()
    {
    }

    protected override PostScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostScheduleRequest> validator)
    {
        return new PostScheduleHandler(logger, dbContext, new IValidator<PostScheduleRequest>[] { validator });
    }

    protected override PostScheduleRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestSeasonId);
    }

    private PostScheduleRequest DefaultRequest(long leagueId, long seasonId)
    {
        var model = new PostScheduleModel()
        {
            Name = testScheduleName
        };
        return new PostScheduleRequest(leagueId, seasonId, DefaultUser(), model);
    }

    protected override void DefaultAssertions(PostScheduleRequest request, ScheduleModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.LeagueId.Should().Be(request.LeagueId);
        result.ScheduleId.Should().NotBe(0);
        result.Name.Should().Be(expected.Name);
        AssertCreated(request.User, DateTime.UtcNow, result);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<ScheduleModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }

    [Theory]
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 0)]
    [InlineData(-42, defaultId)]
    [InlineData(defaultId, -42)]
    public async Task HandleNotFoundAsync(long? leagueId, long? seasonId)
    {
        leagueId ??= TestLeagueId;
        seasonId ??= TestSeasonId;
        var request = DefaultRequest(leagueId.Value, seasonId.Value);
        await base.HandleNotFoundRequestAsync(request);
    }
}
