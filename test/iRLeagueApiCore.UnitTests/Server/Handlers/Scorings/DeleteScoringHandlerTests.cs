using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class DeleteScoringDbTestFixture : HandlersTestsBase<DeleteScoringHandler, DeleteScoringRequest, Unit>
{
    public DeleteScoringDbTestFixture() : base()
    {
    }

    protected override DeleteScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteScoringRequest> validator)
    {
        return new DeleteScoringHandler(logger, dbContext, new IValidator<DeleteScoringRequest>[] { validator });
    }

    protected override DeleteScoringRequest DefaultRequest()
    {
        return DefaultRequest();
    }

    private DeleteScoringRequest DefaultRequest(long leagueId, long scoringId)
    {
        return new DeleteScoringRequest(leagueId, scoringId);
    }

    protected override void DefaultAssertions(DeleteScoringRequest request, Unit result, LeagueDbContext dbContext)
    {
        Assert.DoesNotContain(dbContext.Scorings, x => x.ScoringId == request.ScoringId);
    }

    [Fact]
    public override async Task<Unit> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
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
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
