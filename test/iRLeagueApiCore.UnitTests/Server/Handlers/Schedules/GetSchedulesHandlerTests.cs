using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Schedules;

[Collection("DbTestFixture")]
public class GetSchedulesDbTestFixture : HandlersTestsBase<GetSchedulesHandler, GetSchedulesRequest, IEnumerable<ScheduleModel>>
{
    public GetSchedulesDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override GetSchedulesHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSchedulesRequest> validator)
    {
        return new GetSchedulesHandler(logger, dbContext, new IValidator<GetSchedulesRequest>[] { validator });
    }

    protected override GetSchedulesRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId);
    }

    private GetSchedulesRequest DefaultRequest(long leagueId)
    {
        return new GetSchedulesRequest(leagueId);
    }

    protected override void DefaultAssertions(GetSchedulesRequest request, IEnumerable<ScheduleModel> result, LeagueDbContext dbContext)
    {
        foreach (var dbSchedule in dbContext.Schedules.Where(x => x.LeagueId == testLeagueId))
        {
            Assert.Contains(result, x => x.ScheduleId == dbSchedule.ScheduleId);
        }
    }

    [Fact]
    public override async Task<IEnumerable<ScheduleModel>> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(43)]
    public async Task HandleNotFoundAsync(long leagueId)
    {
        var request = DefaultRequest(leagueId);
        await base.HandleNotFoundRequestAsync(request);
    }
}
