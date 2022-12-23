using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class GetScoringHandlerTest : HandlersTestsBase<GetScoringHandler, GetScoringRequest, ScoringModel>
{
    public GetScoringHandlerTest(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override GetScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScoringRequest> validator)
    {
        return new GetScoringHandler(logger, dbContext, new IValidator<GetScoringRequest>[] { validator });
    }

    protected override GetScoringRequest DefaultRequest()
    {
        return DefaultRequest();
    }

    private GetScoringRequest DefaultRequest(long leagueId = testLeagueId, long scoringId = testScoringId)
    {
        return new GetScoringRequest(leagueId, scoringId);
    }

    protected override void DefaultAssertions(GetScoringRequest request, ScoringModel result, LeagueDbContext dbContext)
    {
        var testScoring = dbContext.Scorings
            .SingleOrDefault(x => x.ScoringId == request.ScoringId);
        result.LeagueId.Should().Be(request.LeagueId);
        result.Id.Should().Be(request.ScoringId);
        result.Name.Should().Be(testScoring.Name);
        result.ExtScoringSourceId.Should().Be(testScoring.ExtScoringSourceId);
        result.MaxResultsPerGroup.Should().Be(testScoring.MaxResultsPerGroup);
        result.ResultConfigId.Should().Be(testScoring.ResultConfigId);
        result.ShowResults.Should().Be(testScoring.ShowResults);
        result.IsCombinedResult.Should().Be(testScoring.IsCombinedResult);
        result.UpdateTeamOnRecalculation.Should().Be(testScoring.UpdateTeamOnRecalculation);
        result.UseResultSetTeam.Should().Be(testScoring.UseResultSetTeam);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<ScoringModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(testLeagueId, 42)]
    [InlineData(2, testScoringId)]
    [InlineData(42, testScoringId)]
    public async Task HandleNotFound(long leagueId, long scoringId)
    {
        var request = DefaultRequest(leagueId, scoringId);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
