using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation.Filters;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;
public sealed class CountRowFilterTests
{
    private static readonly Fixture fixture = new();

    [Theory]
    [InlineData("-1")]
    [InlineData("2")]
    [InlineData("42")]
    public void Constructor_ShouldNotThrow_WithValidValue(string value)
    {
        var test = () => CreateSut([value]);
        test.Should().NotThrow();
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("1e-2")]
    [InlineData("test")]
    public void Constructor_ShouldThrow_WithInvalidValue(string value)
    {
        var test = () => CreateSut([value]);
        test.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(ComparatorType.IsSmaller, 2)]
    [InlineData(ComparatorType.IsSmallerOrEqual, 3)]
    [InlineData(ComparatorType.IsSmallerOrEqual, 2)]
    [InlineData(ComparatorType.IsEqual, 3)]
    [InlineData(ComparatorType.IsBiggerOrEqual, 3)]
    [InlineData(ComparatorType.IsBiggerOrEqual, 4)]
    [InlineData(ComparatorType.IsBigger, 4)]
    [InlineData(ComparatorType.NotEqual, 2)]
    public void FilterRows_ShouldKeepAllRows_WhenConditionMatches(ComparatorType comparatorType, int rowCount)
    {
        var rows = fixture.CreateMany<ResultRowCalculationResult>(rowCount)
            .ToList();
        rows = rows.Shuffle().ToList();
        List<string> filterValues = ["3"];
        var sut = CreateSut(filterValues, comparatorType, MatchedValueAction.Keep);

        var test = sut.FilterRows(rows).ToList();

        test.Should().HaveCount(rowCount);
    }

    [Theory]
    [InlineData(ComparatorType.IsSmaller, 3)]
    [InlineData(ComparatorType.IsSmaller, 4)]
    [InlineData(ComparatorType.IsSmallerOrEqual, 4)]
    [InlineData(ComparatorType.IsEqual, 2)]
    [InlineData(ComparatorType.IsEqual, 4)]
    [InlineData(ComparatorType.IsBiggerOrEqual, 2)]
    [InlineData(ComparatorType.IsBigger, 3)]
    [InlineData(ComparatorType.IsBigger, 2)]
    [InlineData(ComparatorType.NotEqual, 3)]
    public void FilterRows_ShouldRemoveAllRows_WhenConditionMismatches(ComparatorType comparatorType, int rowCount)
    {
        var rows = fixture.CreateMany<ResultRowCalculationResult>(rowCount)
            .ToList();
        rows = rows.Shuffle().ToList();
        List<string> filterValues = ["3"];
        var sut = CreateSut(filterValues, comparatorType, MatchedValueAction.Keep);

        var test = sut.FilterRows(rows).ToList();

        test.Should().HaveCount(0);
    }

    private static CountRowFilter CreateSut(IEnumerable<string> filterValues,
                                 ComparatorType comparator = ComparatorType.IsSmallerOrEqual,
                                 MatchedValueAction action = MatchedValueAction.Keep,
                                 bool allowForEach = false)
    {
        return new CountRowFilter(comparator, filterValues, action, allowForEach);
    }
}
