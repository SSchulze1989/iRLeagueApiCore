using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Schedules;

[Collection("DbTestFixture")]
public sealed class PutScheduleDbTestFixture : HandlersTestsBase<PutScheduleHandler, PutScheduleRequest, ScheduleModel>
{
    private const string testScheduleName = "TestSchedule";

    public PutScheduleDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override PutScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutScheduleRequest> validator)
    {
        return new PutScheduleHandler(logger, dbContext, new IValidator<PutScheduleRequest>[] { validator });
    }

    protected override PutScheduleRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId, testScheduleId);
    }

    private PutScheduleRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
    {
        var model = new PutScheduleModel()
        {
            Name = testScheduleName
        };
        return new PutScheduleRequest(leagueId, DefaultUser(), scheduleId, model);
    }

    protected override void DefaultAssertions(PutScheduleRequest request, ScheduleModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.LeagueId.Should().Be(request.LeagueId);
        result.ScheduleId.Should().Be(request.ScheduleId);
        result.Name.Should().Be(expected.Name);
        AssertChanged(request.User, DateTime.UtcNow, result);
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
    [InlineData(0, testScheduleId)]
    [InlineData(testLeagueId, 0)]
    [InlineData(43, testScheduleId)]
    [InlineData(testLeagueId, 43)]
    public async Task HandleNotFoundAsync(long leagueId, long scheduleId)
    {
        var request = DefaultRequest(leagueId, scheduleId);
        await base.HandleNotFoundRequestAsync(request);
    }
}
