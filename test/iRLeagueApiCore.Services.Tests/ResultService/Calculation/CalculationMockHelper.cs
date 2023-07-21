using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;

internal static class CalculationMockHelper
{
    internal static PointRule<ResultRowCalculationResult> MockPointRule(
        FilterGroupRowFilter<ResultRowCalculationResult>? pointFilters = default,
        FilterGroupRowFilter<ResultRowCalculationResult>? finalFilters = default,
        Func<IEnumerable<ResultRowCalculationResult>, IReadOnlyList<ResultRowCalculationResult>>? sortForPoints = default,
        Func<IEnumerable<ResultRowCalculationResult>, IReadOnlyList<ResultRowCalculationResult>>? sortFinal = default,
        Func<ResultRowCalculationResult, int, double>? getRacePoints = default,
        IDictionary<string, int>? bonusPoints = default)
    {
        pointFilters ??= new(Array.Empty<(FilterCombination, RowFilter<ResultRowCalculationResult>)>());
        finalFilters ??= new(Array.Empty<(FilterCombination, RowFilter<ResultRowCalculationResult>)>());
        sortForPoints ??= row => row.ToList();
        sortFinal ??= row => row.ToList();
        getRacePoints ??= (row, pos) => row.RacePoints;
        bonusPoints ??= new Dictionary<string, int>();
        return MockPointRule<ResultRowCalculationResult>(
            pointFilters,
            finalFilters,
            sortForPoints,
            sortFinal,
            getRacePoints,
            bonusPoints);
    }

    internal static PointRule<T> MockPointRule<T>(
        FilterGroupRowFilter<T> pointFilters,
        FilterGroupRowFilter<T> finalFilters,
        Func<IEnumerable<T>, IReadOnlyList<T>> sortForPoints,
        Func<IEnumerable<T>, IReadOnlyList<T>> sortFinal,
        Func<T, int, double> getRacePoints,
        IDictionary<string, int> bonusPoints) where T : IPointRow, IPenaltyRow
    {
        var mockRule = new Mock<PointRule<T>>();
        mockRule.Setup(x => x.GetPointFilters()).Returns(pointFilters);
        mockRule.Setup(x => x.GetResultFilters()).Returns(finalFilters);
        mockRule
            .Setup(x => x.SortForPoints(It.IsAny<IEnumerable<T>>()))
            .Returns((IEnumerable<T> rows) => sortForPoints(rows).ToList());
        mockRule
            .Setup(x => x.SortFinal(It.IsAny<IEnumerable<T>>()))
            .Returns((IEnumerable<T> rows) => sortFinal(rows).ToList());
        mockRule
            .Setup(x => x.ApplyPoints(It.IsAny<IReadOnlyList<T>>()))
            .Returns((IEnumerable<T> rows) =>
            {
                foreach ((T row, int pos) in rows.Select((x, i) => (x, i + 1)))
                {
                    row.RacePoints = getRacePoints(row, pos);
                }
                return rows.ToList();
            });
        mockRule.Setup(x => x.GetBonusPoints()).Returns(bonusPoints);
        return mockRule.Object;
    }
}
