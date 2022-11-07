using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Calculation;
using AutoFixture.Dsl;
using iRLeagueApiCore.Services.Tests.Extensions;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation
{
    public sealed class MemberSessionCalculationServiceTests
    {
        private readonly Fixture fixture = new();

        [Fact]
        public async Task Calculate_ShouldSetResultData()
        {
            var data = GetCalculationData();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            test.LeagueId.Should().Be(config.LeagueId);
            test.SessionId.Should().Be(config.SessionId);
            test.SessionResultId.Should().Be(config.SessionResultId);
        }

        [Fact]
        public async Task Calculate_ShouldApplyPoints_BasedOnOriginalPosition()
        {
            var data = GetCalculationData();
            data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            config.PointRule = MockPointRule(getRacePoints: (row, pos) => pos);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

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
            var sut = fixture.Create<MemberSessionCalculationService>();

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
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            test.ResultRows.Should().BeInAscendingOrder(x => x.FinalPosition);
        }

        [Fact]
        public async Task Calculate_ShouldSetFinalPositionAndChange()
        {
            var data = GetCalculationData();
            data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            var expectedFinalPositions = Enumerable.Range(1, data.ResultRows.Count());
            var expectedFinalPositionChanges = data.ResultRows.Select((x, i) => (int)(x.StartPosition - (i + 1)));
            test.ResultRows.Select(x => x.FinalPosition).Should().BeEquivalentTo(expectedFinalPositions);
            test.ResultRows.Select(x => x.FinalPositionChange).Should().BeEquivalentTo(expectedFinalPositionChanges);
        }

        [Fact]
        public async Task Calculate_ShouldSetFastestLap()
        {
            var data = GetCalculationData();
            var rows = data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            var expectedLapRow = rows.MinBy(x => x.FastestLapTime)!;
            test.FastestLap.Should().Be(expectedLapRow.FastestLapTime);
            test.FastestLapDriverMemberId.Should().Be(expectedLapRow.MemberId);
        }

        [Fact]
        public async Task Calculate_ShouldSetFastestAvgLap()
        {
            var data = GetCalculationData();
            var rows = data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            var expectedLapRow = rows.MinBy(x => x.AvgLapTime)!;
            test.FastestAvgLap.Should().Be(expectedLapRow.AvgLapTime);
            test.FastestAvgLapDriverMemberId.Should().Be(expectedLapRow.MemberId);
        }

        [Fact]
        public async Task Calculate_ShouldSetFastestQualyLap()
        {
            var data = GetCalculationData();
            var rows = data.ResultRows = GetTestRows();
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            var expectedLapRow = rows.MinBy(x => x.QualifyingTime)!;
            test.FastestQualyLap.Should().Be(expectedLapRow.QualifyingTime);
            test.FastestQualyLapDriverMemberId.Should().Be(expectedLapRow.MemberId);
        }

        [Fact]
        public async Task Calculate_ShouldSetHardChargers()
        {
            const int rowCount = 3;
            var startPositions = new[] { 3, 2, 5 }.AsEnumerable().GetEnumerator();
            var finishPositions = new[] { 1, 2, 3 }.AsEnumerable().GetEnumerator();
            var data = GetCalculationData();
            var rows = data.ResultRows = TestRowBuilder()
                .With(x => x.StartPosition,() => startPositions.Next())
                .With(x => x.FinishPosition,() => finishPositions.Next())
                .CreateMany(rowCount);
            var pointRule = MockPointRule(
                sortFinal: rows => rows.OrderBy(x => x.FinishPosition).ToList());
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            var expectedHardChargers = new[] { rows.ElementAt(0), rows.ElementAt(2) }.Select(x => x.MemberId);
            test.HardChargers.Should().BeEquivalentTo(expectedHardChargers);
        }

        [Fact]
        public async Task Calculate_ShouldSetCleanestDrivers()
        {
            const int rowCount = 3;
            var incidents = new[] { 1, 2, 1 }.AsEnumerable().GetEnumerator();
            var data = GetCalculationData();
            var rows = data.ResultRows = TestRowBuilder()
                .With(x => x.Incidents, () => incidents.Next())
                .CreateMany(rowCount);
            var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
            fixture.Register(() => config);
            var sut = fixture.Create<MemberSessionCalculationService>();

            var test = await sut.Calculate(data);

            var expectedCleanestDrivers = new[] { rows.ElementAt(0), rows.ElementAt(2) }.Select(x => x.MemberId);
            test.CleanestDrivers.Should().BeEquivalentTo(expectedCleanestDrivers);
        }

        private SessionCalculationData GetCalculationData()
        {
            return fixture.Create<SessionCalculationData>();
        }

        private SessionCalculationConfiguration GetCalculationConfiguration(long leagueId, long? sessionId)
        {
            return fixture
                .Build<SessionCalculationConfiguration>()
                .With(x => x.LeagueId, leagueId)
                .With(x => x.SessionId, sessionId)
                .Create();
        }

        private IPostprocessComposer<ResultRowCalculationData> TestRowBuilder()
        {
            return fixture.Build<ResultRowCalculationData>()
                .Without(x => x.RacePoints)
                .Without(x => x.BonusPoints)
                .Without(x => x.PenaltyPoints)
                .Without(x => x.AddPenalty);
        }

        private IEnumerable<ResultRowCalculationData> GetTestRows()
        {
            return TestRowBuilder().CreateMany();
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
            mockRule.Setup(x => x.GetPointFilters()).Returns(pointFilters);
            mockRule.Setup(x => x.GetFinalFilters()).Returns(finalFilters);
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
