using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Calculation;
using AutoFixture.Dsl;
using iRLeagueApiCore.Services.Tests.Extensions;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;

public sealed class MemberSessionCalculationServiceTests
{
    private readonly Fixture fixture = new();

    [Fact]
    public async Task Calculate_ShouldSetResultMetaData()
    {
        var data = GetCalculationData();
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.LeagueId.Should().Be(config.LeagueId);
        test.Name.Should().Be(config.Name);
        test.SessionId.Should().Be(config.SessionId);
        test.SessionResultId.Should().Be(config.SessionResultId);
    }

    [Fact]
    public async Task Calculate_ShouldApplyPoints_BasedOnOriginalPosition()
    {
        var data = GetCalculationData();
        data.ResultRows = GetTestRows();
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        config.PointRule = CalculationMockHelper.MockPointRule(getRacePoints: (row, pos) => pos);
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.ResultRows.Should().HaveSameCount(data.ResultRows);
        foreach ((var row, var pos) in test.ResultRows.Select((x, i) => (x, i + 1)))
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
        config.PointRule = CalculationMockHelper.MockPointRule(
            sortForPoints: rows => rows.OrderBy(x => x.FinishPosition).ToList(),
            getRacePoints: (row, pos) => pos);
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.ResultRows.Should().HaveSameCount(data.ResultRows);
        test.ResultRows.Select(x => x.FinishPosition).Should()
            .BeEquivalentTo(data.ResultRows.Select(x => x.FinishPosition));
        foreach ((var row, var pos) in test.ResultRows.OrderBy(x => x.FinishPosition).Select((x, i) => (x, i + 1)))
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
        config.PointRule = CalculationMockHelper.MockPointRule(
            sortFinal: rows => rows.OrderBy(x => x.FinishPosition).ToList());
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.ResultRows.Should().BeInAscendingOrder(x => x.FinishPosition);
    }

    [Fact]
    public async Task Calculate_ShouldSetFinalPositionAndChange()
    {
        var data = GetCalculationData();
        data.ResultRows = GetTestRows();
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        fixture.Register(() => config);
        var sut = CreateSut();

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
        var sut = CreateSut();

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
        var sut = CreateSut();

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
        var sut = CreateSut();

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
            .With(x => x.StartPosition, () => startPositions.Next())
            .With(x => x.FinishPosition, () => finishPositions.Next())
            .CreateMany(rowCount);
        var pointRule = CalculationMockHelper.MockPointRule(
            sortFinal: rows => rows.OrderBy(x => x.FinishPosition).ToList());
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        fixture.Register(() => config);
        var sut = CreateSut();

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
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        var expectedCleanestDrivers = new[] { rows.ElementAt(0), rows.ElementAt(2) }.Select(x => x.MemberId);
        test.CleanestDrivers.Should().BeEquivalentTo(expectedCleanestDrivers);
    }

    [Fact]
    public async Task Calculate_ShoulNotThrow_WhenColumnsHaveDefaultValues()
    {
        const int rowCount = 3;
        var data = GetCalculationData();
        var rows = data.ResultRows = TestRowBuilder()
            .OmitAutoProperties()
            .With(x => x.MemberId)
            .With(x => x.Firstname)
            .With(x => x.Lastname)
            .CreateMany(rowCount);
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = async () => await sut.Calculate(data);

        await test.Should().NotThrowAsync();
    }

    private MemberSessionCalculationService CreateSut()
    {
        return fixture.Create<MemberSessionCalculationService>();
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
}
