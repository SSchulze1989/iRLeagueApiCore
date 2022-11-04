using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Calculation;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation
{
    public class MemberSessionResultCalculationServiceTests
    {
        private readonly Fixture fixture = new();

        [Fact]
        public async Task Calculate_ShouldApplyPoints_BasedOnOriginalPosition()
        {
            var data = GetCalculationData();
            data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            config.PointRule = MockPointRule(getRacePoints: (row, pos) => pos);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionResultCalculationService>();

            var test = await sut.Calculate(data);

            test.ResultRows.Should().HaveSameCount(data.ResultRows);
            foreach((var row, var pos) in test.ResultRows.Select((x, i) => (x, i+1)))
            {
                row.RacePoints.Should().Be(pos);
                row.TotalPoints.Should().Be(row.RacePoints);
            }
        }

        [Fact]
        public async Task Calculate_ShouldApplyPoints_BasedOnSortedPosition()
        {
            var data = GetCalculationData();
            data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            config.PointRule = MockPointRule(
                sortForPoints: rows => rows.OrderBy(x => x.FinishPosition).ToList(),
                getRacePoints: (row, pos) => pos);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionResultCalculationService>();

            var test = await sut.Calculate(data);

            test.ResultRows.Should().HaveSameCount(data.ResultRows);
            test.ResultRows.Select(x => x.FinishPosition).Should()
                .BeEquivalentTo(data.ResultRows.Select(x => x.FinishPosition));
            foreach((var row, var pos) in test.ResultRows.OrderBy(x => x.FinishPosition).Select((x, i) => (x, i+1)))
            {
                row.RacePoints.Should().Be(pos);
            }
        }

        [Fact]
        public async Task Calculate_ShouldSortFinal()
        {
            var data = GetCalculationData();
            data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            config.PointRule = MockPointRule(
                sortFinal: rows => rows.OrderBy(x => x.FinalPosition).ToList());
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionResultCalculationService>();

            var test = await sut.Calculate(data);

            test.ResultRows.Should().BeInAscendingOrder(x => x.FinalPosition);
        }

        private SessionResultCalculationData GetCalculationData()
        {
            return fixture.Create<SessionResultCalculationData>();
        }

        private SessionResultCalculationConfiguration GetCalculationConfiguration(long leagueId, long? sessionId)
        {
            return fixture
                .Build<SessionResultCalculationConfiguration>()
                .With(x => x.LeagueId, leagueId)
                .With(x => x.SessionId, sessionId)
                .Create();
        }

        private IEnumerable<ResultRowCalculationData> GetTestRows()
        {
            return fixture.Build<ResultRowCalculationData>()
                .Without(x => x.RacePoints)
                .Without(x => x.BonusPoints)
                .Without(x => x.PenaltyPoints)
                .Without(x => x.AddPenalty)
                .CreateMany();
        }

        private static PointRule<ResultRowCalculationResult> MockPointRule(
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
            getPenaltyPoints ??= (row, pos) => row.PenaltyPoints + row.AddPenalty?.PenaltyPoints ?? 0;
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

        private static PointRule<T> MockPointRule<T>(
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
            mockRule.SetupGet(x => x.PointFilters).Returns(pointFilters);
            mockRule.SetupGet(x => x.FinalFilters).Returns(finalFilters);
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
