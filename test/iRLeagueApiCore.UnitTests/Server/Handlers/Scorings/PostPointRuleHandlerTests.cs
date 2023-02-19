using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class PostPointRuleDbTestFixture : HandlersTestsBase<PostPointRuleHandler, PostPointRuleRequest, PointRuleModel>
{
    public PostPointRuleDbTestFixture() : base()
    {
    }

    protected override PostPointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostPointRuleRequest> validator)
    {
        return new PostPointRuleHandler(logger, dbContext, new IValidator<PostPointRuleRequest>[] { validator });
    }

    private PostPointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
    {
        var PostPointRule = new PostPointRuleModel()
        {
            Name = "TestPointRule",
            BonusPoints = new Dictionary<string, int>() { { "p1", 1 }, { "q1", 1 } },
            FinalSortOptions = new SortOptions[] { SortOptions.PenPtsAsc, SortOptions.TotalPtsAsc },
            MaxPoints = 10,
            PointDropOff = 1,
            PointsPerPlace = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
            PointsSortOptions = new SortOptions[] { SortOptions.IntvlAsc, SortOptions.TotalPtsAsc },
        };
        return new PostPointRuleRequest(leagueId, DefaultUser(), PostPointRule);
    }

    protected override PostPointRuleRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestPointRuleId);
    }

    protected override void DefaultAssertions(PostPointRuleRequest request, PointRuleModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.PointRuleId.Should().NotBe(0);
        result.BonusPoints.Should().BeEquivalentTo(expected.BonusPoints);
        result.FinalSortOptions.Should().BeEquivalentTo(expected.FinalSortOptions);
        result.LeagueId.Should().Be(request.LeagueId);
        result.MaxPoints.Should().Be(expected.MaxPoints);
        result.Name.Should().Be(expected.Name);
        result.PointDropOff.Should().Be(expected.PointDropOff);
        result.PointsPerPlace.Should().BeEquivalentTo(expected.PointsPerPlace);
        result.PointsSortOptions.Should().BeEquivalentTo(expected.PointsSortOptions);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<PointRuleModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
