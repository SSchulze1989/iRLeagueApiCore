using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation
{
    internal static class CalculationMockHelper
    {
        internal static PointRule<ResultRowCalculationResult> MockPointRule(
            IEnumerable<RowFilter<ResultRowCalculationResult>>? pointFilters = default,
            IEnumerable<RowFilter<ResultRowCalculationResult>>? finalFilters = default,
            Func<IEnumerable<ResultRowCalculationResult>, IReadOnlyList<ResultRowCalculationResult>>? sortForPoints = default,
            Func<IEnumerable<ResultRowCalculationResult>, IReadOnlyList<ResultRowCalculationResult>>? sortFinal = default,
            Func<ResultRowCalculationResult, int, double>? getRacePoints = default,
            Func<ResultRowCalculationResult, int, double>? getBonusPoints = default,
            Func<ResultRowCalculationResult, int, double>? getPenaltyPoints = default,
            Func<ResultRowCalculationResult, int, double>? getTotalPoints = default)
        {
            pointFilters ??= Array.Empty<RowFilter<ResultRowCalculationResult>>();
            finalFilters ??= Array.Empty<RowFilter<ResultRowCalculationResult>>();
            sortForPoints ??= row => row.ToList();
            sortFinal ??= row => row.ToList();
            getRacePoints ??= (row, pos) => row.RacePoints;
            getBonusPoints ??= (row, pos) => row.BonusPoints;
            getPenaltyPoints ??= (row, pos) => row.PenaltyPoints + (row.AddPenalty?.PenaltyPoints ?? 0);
            getTotalPoints ??= (row, pos) => row.RacePoints + row.BonusPoints - row.PenaltyPoints;
            return MockPointRule<ResultRowCalculationResult>(
                pointFilters,
                finalFilters,
                sortForPoints,
                sortFinal,
                getRacePoints,
                getBonusPoints,
                getPenaltyPoints,
                getTotalPoints);
        }

        internal static PointRule<T> MockPointRule<T>(
            IEnumerable<RowFilter<T>> pointFilters,
            IEnumerable<RowFilter<T>> finalFilters,
            Func<IEnumerable<T>, IReadOnlyList<T>> sortForPoints,
            Func<IEnumerable<T>, IReadOnlyList<T>> sortFinal,
            Func<T, int, double> getRacePoints,
            Func<T, int, double> getBonusPoints,
            Func<T, int, double> getPenaltyPoints,
            Func<T, int, double> getTotalPoints) where T : IPointRow, IPenaltyRow
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
                        row.BonusPoints = getBonusPoints(row, pos);
                        row.PenaltyPoints = getPenaltyPoints(row, pos);
                        row.TotalPoints = getTotalPoints(row, pos);
                    }
                    return rows.ToList();
                });
            return mockRule.Object;
        }
    }
}
