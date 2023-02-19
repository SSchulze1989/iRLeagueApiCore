using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class GetPointRuleDbTestFixture : HandlersTestsBase<GetPointRuleHandler, GetPointRuleRequest, PointRuleModel>
{
    public GetPointRuleDbTestFixture() : base()
    {
    }

    protected override GetPointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetPointRuleRequest> validator)
    {
        return new GetPointRuleHandler(logger, dbContext, new IValidator<GetPointRuleRequest>[] { validator });
    }

    private GetPointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
    {
        return new GetPointRuleRequest(leagueId, pointRuleId);
    }

    protected override GetPointRuleRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestPointRuleId);
    }

    protected override void DefaultAssertions(GetPointRuleRequest request, PointRuleModel result, LeagueDbContext dbContext)
    {
        var testPointRule = dbContext.PointRules
            .First(x => x.PointRuleId == request.PointRuleId);
        result.PointRuleId.Should().Be(request.PointRuleId);
        result.BonusPoints.Should().BeEquivalentTo(testPointRule!.BonusPoints);
        result.FinalSortOptions.Should().BeEquivalentTo(testPointRule.FinalSortOptions);
        result.LeagueId.Should().Be(request.LeagueId);
        result.MaxPoints.Should().Be(testPointRule.MaxPoints);
        result.Name.Should().Be(testPointRule.Name);
        result.PointDropOff.Should().Be(testPointRule.PointDropOff);
        result.PointsPerPlace.Should().BeEquivalentTo(testPointRule.PointsPerPlace);
        result.PointsSortOptions.Should().BeEquivalentTo(testPointRule.PointsSortOptions);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<PointRuleModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 0)]
    [InlineData(42, defaultId)]
    [InlineData(defaultId, 42)]
    public async Task ShouldHandleNotFound(long? leagueId, long? pointRuleId)
    {
        leagueId ??= TestLeagueId;
        pointRuleId ??= TestPointRuleId;
        var request = DefaultRequest(leagueId.Value, pointRuleId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
