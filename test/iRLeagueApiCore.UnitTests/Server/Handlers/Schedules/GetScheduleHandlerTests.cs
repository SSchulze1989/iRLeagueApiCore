using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Schedules;

[Collection("DbTestFixture")]
public sealed class GetScheduleDbTestFixture : HandlersTestsBase<GetScheduleHandler, GetScheduleRequest, ScheduleModel>
{
    public GetScheduleDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override GetScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScheduleRequest> validator)
    {
        return new GetScheduleHandler(logger, dbContext, new IValidator<GetScheduleRequest>[] { validator });
    }

    protected override GetScheduleRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId, testScheduleId);
    }

    private GetScheduleRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
    {
        return new GetScheduleRequest(leagueId, scheduleId);
    }

    protected override void DefaultAssertions(GetScheduleRequest request, ScheduleModel result, LeagueDbContext dbContext)
    {
        result.LeagueId.Should().Be(request.LeagueId);
        result.ScheduleId.Should().Be(request.ScheduleId);
        var scheduleEntity = dbContext.Schedules
            .Include(x => x.Events)
            .SingleOrDefault(x => x.ScheduleId == result.ScheduleId);
        scheduleEntity.Should().NotBeNull();
        result.Name.Should().Be(scheduleEntity.Name);
        result.EventIds.Should().BeEquivalentTo(scheduleEntity.Events.Select(x => x.EventId));
        result.SeasonId.Should().Be(scheduleEntity.SeasonId);
        AssertVersion(scheduleEntity, result);
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
