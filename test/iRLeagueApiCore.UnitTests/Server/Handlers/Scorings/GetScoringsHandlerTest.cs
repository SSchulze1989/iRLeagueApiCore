using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class GetScoringsHandlerTest : HandlersTestsBase<GetScoringsHandler, GetScoringsRequest, IEnumerable<ScoringModel>>
{
    public GetScoringsHandlerTest() : base()
    {
    }

    protected override GetScoringsHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScoringsRequest> validator)
    {
        return new GetScoringsHandler(logger, dbContext, new IValidator<GetScoringsRequest>[] { validator });
    }

    protected override GetScoringsRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId);
    }

    private GetScoringsRequest DefaultRequest(long leagueId)
    {
        return new GetScoringsRequest(leagueId);
    }

    protected override void DefaultAssertions(GetScoringsRequest request, IEnumerable<ScoringModel> result, LeagueDbContext dbContext)
    {
        base.DefaultAssertions(request, result, dbContext);
        Assert.NotEmpty(result);
    }

    [Fact]
    public override async Task<IEnumerable<ScoringModel>> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
