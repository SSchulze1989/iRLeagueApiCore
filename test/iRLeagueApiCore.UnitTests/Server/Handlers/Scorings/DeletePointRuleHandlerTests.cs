using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class DeletePointRuleDbTestFixture : HandlersTestsBase<DeletePointRuleHandler, DeletePointRuleRequest, MediatR.Unit>
{
    public DeletePointRuleDbTestFixture() : base()
    {
    }

    protected override DeletePointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeletePointRuleRequest> validator)
    {
        return new DeletePointRuleHandler(logger, dbContext, new IValidator<DeletePointRuleRequest>[] { validator });
    }

    private DeletePointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
    {
        return new DeletePointRuleRequest(leagueId, pointRuleId);
    }

    protected override DeletePointRuleRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestPointRuleId);
    }

    protected override void DefaultAssertions(DeletePointRuleRequest request, MediatR.Unit result, LeagueDbContext dbContext)
    {
        var testPointRule = dbContext.PointRules
            .SingleOrDefault(x => x.PointRuleId == request.PointRuleId);
        testPointRule.Should().BeNull();
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<MediatR.Unit> ShouldHandleDefault()
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
