﻿using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings;

[Collection("DbTestFixture")]
public sealed class PutPointRuleDbTestFixture : HandlersTestsBase<PutPointRuleHandler, PutPointRuleRequest, PointRuleModel>
{
    public PutPointRuleDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override PutPointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutPointRuleRequest> validator)
    {
        return new PutPointRuleHandler(logger, dbContext, new IValidator<PutPointRuleRequest>[] { validator });
    }

    private PutPointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
    {
        var putPointRule = new PutPointRuleModel()
        {
            Name = "TestPointRule",
            BonusPoints = new Dictionary<string, int>() { { "p1", 1 }, { "q1", 1 } },
            FinalSortOptions = new SortOptions[] { SortOptions.PenPtsAsc, SortOptions.TotalPtsAsc },
            MaxPoints = 10,
            PointDropOff = 1,
            PointsPerPlace = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
            PointsSortOptions = new SortOptions[] { SortOptions.IntvlAsc, SortOptions.TotalPtsAsc },
        };
        return new PutPointRuleRequest(leagueId, pointRuleId, DefaultUser(), putPointRule);
    }

    protected override PutPointRuleRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId, testPointRuleId);
    }

    protected override void DefaultAssertions(PutPointRuleRequest request, PointRuleModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.PointRuleId.Should().Be(request.PointRuleId);
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

    [Theory]
    [InlineData(0, testPointRuleId)]
    [InlineData(testLeagueId, 0)]
    [InlineData(42, testPointRuleId)]
    [InlineData(testLeagueId, 42)]
    public async Task ShouldHandleNotFound(long leagueId, long pointRuleId)
    {
        var request = DefaultRequest(leagueId, pointRuleId);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
