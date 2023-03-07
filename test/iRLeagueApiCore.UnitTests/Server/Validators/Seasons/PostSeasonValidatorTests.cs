using FluentValidation.TestHelper;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Server.Validation.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Validators.Seasons;

public sealed class PostSeasonDbTestFixture : IClassFixture<DbTestFixture>
{
    private readonly DbTestFixture fixture;

    private long TestLeagueId => fixture.DbContext.Leagues.First().Id;
    private const string testSeasonName = "TestSeason";

    public PostSeasonDbTestFixture(DbTestFixture fixture)
    {
        this.fixture = fixture;
    }

    private PostSeasonRequest DefaultRequest(long? leagueId = null)
    {
        leagueId ??= TestLeagueId;
        var model = new PostSeasonModel()
        {
            HideComments = true,
            MainScoringId = null,
            Finished = true,
            SeasonName = testSeasonName,
        };
        return new PostSeasonRequest(leagueId.Value, LeagueUser.Empty, model);
    }

    private static PostSeasonRequestValidator CreateValidator(LeagueDbContext dbContext)
    {
        return new PostSeasonRequestValidator(dbContext, new PostSeasonModelValidator());
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(0, false)]
    public async Task ValidateLeagueId(long? leagueId, bool expectValid)
    {
        leagueId ??= TestLeagueId;
        var dbContext = fixture.CreateDbContext();
        var request = DefaultRequest(leagueId);
        var validator = CreateValidator(dbContext);
        var result = await validator.TestValidateAsync(request);
        Assert.Equal(expectValid, result.IsValid);
        if (expectValid == false)
        {
            result.ShouldHaveValidationErrorFor(x => x.LeagueId);
        }
    }

    [Theory]
    [InlineData("ValidName", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task ValidateSeasonName(string name, bool expectValid)
    {
        var dbContext = fixture.CreateDbContext();
        var request = DefaultRequest();
        request.Model.SeasonName = name;
        var validator = CreateValidator(dbContext);
        var result = await validator.TestValidateAsync(request);
        Assert.Equal(expectValid, result.IsValid);
        if (expectValid == false)
        {
            result.ShouldHaveValidationErrorFor(x => x.Model.SeasonName);
        }
    }
}
