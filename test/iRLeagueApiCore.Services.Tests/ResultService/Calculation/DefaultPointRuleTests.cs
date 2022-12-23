using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;

public class DefaultPointRuleTests
{
    private readonly Fixture fixture = new();

    [Fact]
    public void SortForPoints_ShouldNotChangeOrder()
    {
        var rows = GetTestRows(fixture);
        var sut = GetPointRule(fixture);

        var test = sut.SortForPoints(rows.ToList());

        test.Should().BeEquivalentTo(rows);
    }

    [Fact]
    public void ApplyPoints_ShouldCalculateTotalPoints()
    {
        var rows = GetTestRows(fixture);
        var sut = GetPointRule(fixture);

        var test = sut.ApplyPoints(rows.ToList());

        foreach (var row in test)
        {
            row.TotalPoints.Should().Be(row.RacePoints + row.BonusPoints - row.PenaltyPoints);
        }
    }

    [Fact]
    public void ApplyPoints_ShouldCalculatePenaltyPoints_WhenAddPenaltyExists()
    {
        var rows = GetTestRows(fixture);
        double penaltyPoints = rows.ElementAt(0).PenaltyPoints = fixture.Create<double>();
        var addPenalty = rows.ElementAt(0).AddPenalty = fixture.Create<AddPenaltyCalculationData>();
        var sut = GetPointRule(fixture);

        var test = sut.ApplyPoints(rows.ToList()).ElementAt(0);

        test.PenaltyPoints.Should().Be(addPenalty.PenaltyPoints);
    }

    private static IEnumerable<ResultRowCalculationData> GetTestRows(Fixture fixture)
    {
        return fixture
            .Build<ResultRowCalculationData>()
            .Without(x => x.AddPenalty)
            .CreateMany(10).ToList();
    }

    private static DefaultPointRule<ResultRowCalculationData> GetPointRule(Fixture fixture)
    {
        return fixture.Create<DefaultPointRule<ResultRowCalculationData>>();
    }
}
