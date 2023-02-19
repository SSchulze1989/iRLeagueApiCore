using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class PutScoringDbTestFixture : HandlersTestsBase<PutScoringHandler, PutScoringRequest, ScoringModel>
{
    private const string NewScoringName = "New scoring Name";

    public PutScoringDbTestFixture() : base()
    {
    }

    protected override PutScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutScoringRequest> validator)
    {
        return new PutScoringHandler(logger, dbContext, new IValidator<PutScoringRequest>[] { validator });
    }

    private PutScoringRequest DefaultRequest(long leagueId, long scoringId)
    {
        var model = new PutScoringModel()
        {
            Name = NewScoringName,
            ShowResults = true,
            IsCombinedResult = true,
        };
        return new PutScoringRequest(leagueId, scoringId, DefaultUser(), model);
    }

    protected override PutScoringRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestScoringId);
    }

    protected override void DefaultAssertions(PutScoringRequest request, ScoringModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.LeagueId.Should().Be(request.LeagueId);
        result.Id.Should().Be(request.ScoringId);
        result.Name.Should().Be(expected.Name);
        result.ScoringKind.Should().Be(expected.ScoringKind);
        result.ExtScoringSourceId.Should().Be(expected.ExtScoringSourceId);
        result.MaxResultsPerGroup.Should().Be(expected.MaxResultsPerGroup);
        result.ShowResults.Should().Be(expected.ShowResults);
        result.IsCombinedResult.Should().Be(expected.IsCombinedResult);
        result.UpdateTeamOnRecalculation.Should().Be(expected.UpdateTeamOnRecalculation);
        result.UseResultSetTeam.Should().Be(expected.UseResultSetTeam);
        AssertChanged(request.User, DateTime.UtcNow, result);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<ScoringModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }

    [Theory]
    [InlineData(defaultId, 0)]
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 42)]
    [InlineData(42, defaultId)]
    public async Task HandleNotFoundAsync(long? leagueId, long? scoringId)
    {
        leagueId ??= TestLeagueId;
        scoringId ??= TestScoringId;
        var request = DefaultRequest(leagueId.Value, scoringId.Value);
        await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await HandleSpecialAsync(request, null));
    }
}
