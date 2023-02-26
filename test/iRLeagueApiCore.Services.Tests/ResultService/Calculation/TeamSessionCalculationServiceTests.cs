using AutoFixture.Dsl;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Mocking.Extensions;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;

public sealed class TeamSessionCalculationServiceTests
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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task Calculate_ShouldGroupTeams_WithMaxNrOfRows(int groupRowCount)
    {
        var teamCount = 3;
        var teamIds = fixture.CreateMany<long?>(teamCount).Concat(new[] { default(long?) });
        var rowsPerTeam = 3;
        var data = GetCalculationData();
        data.ResultRows = GetTestRows(teamIds, rowsPerTeam);
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        config.MaxResultsPerGroup = groupRowCount;
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.ResultRows.Should().HaveCount(teamCount);
        foreach (var teamRow in test.ResultRows)
        {
            teamRow.ScoredMemberResultRowIds.Should().HaveCount(Math.Min(rowsPerTeam, groupRowCount));
            teamRow.ScoredMemberResultRowIds.OrderBy(x => x).Should()
                .BeEquivalentTo(data.ResultRows
                    .Where(x => x.TeamId == teamRow.TeamId)
                    .OrderBy(x => x.FinalPosition)
                    .Take(groupRowCount)
                    .Select(x => x.ScoredResultRowId)
                    .OrderBy(x => x)
                );
        }
    }

    [Fact]
    public async Task Calculate_ShouldAccumulateTeamResultData()
    {
        var groupRowCount = 3;
        var teamCount = 3;
        var teamIds = fixture.CreateMany<long?>(teamCount);
        var rowsPerTeam = 5;
        var data = GetCalculationData();
        data.ResultRows = GetTestRows(teamIds, rowsPerTeam);
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        config.MaxResultsPerGroup = groupRowCount;
        config.PointRule = CalculationMockHelper.MockPointRule();
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.ResultRows.Should().HaveCount(teamCount);
        foreach (var teamRow in test.ResultRows)
        {
            var memberRows = data.ResultRows
                .Where(x => x.TeamId == teamRow.TeamId)
                .OrderBy(x => x.FinalPosition)
                .Take(groupRowCount);
            teamRow.AvgLapTime.Should().BeCloseTo(TimeSpan.FromSeconds(memberRows.Sum(x => x.AvgLapTime.TotalSeconds * x.CompletedLaps) / memberRows.Sum(x => x.CompletedLaps)), TimeSpan.FromMilliseconds(1));
            teamRow.BonusPoints.Should().Be(0);
            teamRow.CompletedLaps.Should().Be(memberRows.Sum(x => x.CompletedLaps));
            teamRow.FastestLapTime.Should().Be(memberRows.Min(x => x.FastestLapTime));
            teamRow.MemberId.Should().BeNull();
            teamRow.Firstname.Should().BeEmpty();
            teamRow.Lastname.Should().BeEmpty();
            teamRow.Incidents.Should().Be(memberRows.Sum(x => x.Incidents));
            teamRow.Interval.Should().BeCloseTo(TimeSpan.FromSeconds(memberRows.Sum(x => x.Interval.TotalSeconds)), TimeSpan.FromMilliseconds(1));
            teamRow.LeadLaps.Should().Be(memberRows.Sum(x => x.LeadLaps));
            teamRow.PenaltyPoints.Should().Be(memberRows.Sum(x => x.PenaltyPoints));
            teamRow.QualifyingTime.Should().Be(memberRows.Min(x => x.QualifyingTime));
            teamRow.RacePoints.Should().Be(memberRows.Sum(x => x.RacePoints + x.BonusPoints));
        }
    }

    [Fact]
    public async Task Calculate_ShouldCalculateTotalPoints()
    {
        var groupRowCount = 3;
        var teamCount = 3;
        var teamIds = fixture.CreateMany<long?>(teamCount);
        var rowsPerTeam = 5;
        var data = GetCalculationData();
        data.ResultRows = GetTestRows(teamIds, rowsPerTeam);
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        config.PointRule = CalculationMockHelper.MockPointRule();
        config.MaxResultsPerGroup = groupRowCount;
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        foreach (var row in test.ResultRows)
        {
            row.TotalPoints.Should().Be(row.RacePoints + row.BonusPoints - row.PenaltyPoints);
        }
    }

    [Fact]
    public async Task Calculate_ShouldSortFinal()
    {
        var teamCount = 3;
        var teamIds = fixture.CreateMany<long?>(teamCount);
        var rowsPerTeam = 5;
        var data = GetCalculationData();
        data.ResultRows = GetTestRows(teamIds, rowsPerTeam);
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        config.PointRule = CalculationMockHelper.MockPointRule(
            sortFinal: rows => rows.OrderBy(x => x.TotalPoints).ToList());
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.ResultRows.Should().BeInAscendingOrder(x => x.TotalPoints);
    }

    [Fact]
    public async Task Calculate_ShouldSetFinalPosition()
    {
        var teamCount = 3;
        var teamIds = fixture.CreateMany<long?>(teamCount);
        var rowsPerTeam = 5;
        var data = GetCalculationData();
        data.ResultRows = GetTestRows(teamIds, rowsPerTeam);
        var config = GetCalculationConfiguration(data.LeagueId, data.SessionId);
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        var expectedFinalPositions = Enumerable.Range(1, teamCount);
        test.ResultRows.Select(x => x.FinalPosition).Should().BeEquivalentTo(expectedFinalPositions);
    }

    private TeamSessionCalculationService CreateSut()
    {
        return fixture.Create<TeamSessionCalculationService>();
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
            .With(x => x.ResultKind, ResultKind.Team)
            .Create();
    }

    private IPostprocessComposer<ResultRowCalculationData> TestRowBuilder()
    {
        return fixture.Build<ResultRowCalculationData>()
            .Without(x => x.AddPenalty);
    }

    private IEnumerable<ResultRowCalculationData> GetTestRows(IEnumerable<long?> teamIds, int rowsPerTeam)
    {
        var idSequence = teamIds.CreateSequence();
        return TestRowBuilder()
            .With(x => x.TeamId, () => idSequence())
            .CreateMany(teamIds.Count() * rowsPerTeam);
    }
}
