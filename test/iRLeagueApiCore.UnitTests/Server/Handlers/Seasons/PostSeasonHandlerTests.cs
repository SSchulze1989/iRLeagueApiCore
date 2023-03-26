using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Seasons;

public sealed class PostSeasonDbTestFixture : HandlersTestsBase<PostSeasonHandler, PostSeasonRequest, SeasonModel>
{
    private const string testSeasonName = "TestSeason";

    public PostSeasonDbTestFixture() : base()
    {
    }

    protected override PostSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostSeasonRequest> validator)
    {
        return new PostSeasonHandler(logger, dbContext, new IValidator<PostSeasonRequest>[] { validator });
    }

    protected override PostSeasonRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId);
    }

    private PostSeasonRequest DefaultRequest(long leagueId)
    {
        var model = new PostSeasonModel()
        {
            SeasonName = testSeasonName
        };
        return new PostSeasonRequest(leagueId, DefaultUser(), model);
    }

    protected override void DefaultAssertions(PostSeasonRequest request, SeasonModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.LeagueId.Should().Be(request.LeagueId);
        result.SeasonId.Should().NotBe(0);
        result.Finished.Should().Be(expected.Finished);
        result.HideComments.Should().Be(expected.HideComments);
        result.MainScoringId.Should().Be(expected.MainScoringId);
        result.SeasonName.Should().Be(expected.SeasonName);
        AssertCreated(request.User, DateTime.UtcNow, result);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public async override Task<SeasonModel> ShouldHandleDefault()
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
    [InlineData(-42)]
    public async Task HandleNotFound(long leagueId)
    {
        var request = DefaultRequest(leagueId);
        await HandleNotFoundRequestAsync(request);
    }
}
