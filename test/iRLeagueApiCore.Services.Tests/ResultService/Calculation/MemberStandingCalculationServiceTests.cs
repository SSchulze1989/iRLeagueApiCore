﻿using AutoFixture.Dsl;
using DbIntegrationTests;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Mocking.Extensions;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;

public sealed class MemberStandingCalculationServiceTests
{
    private readonly Fixture fixture = new();

    public static IEnumerable<object[]> TestSortOptions()
    {
        yield return new object[] { SortOptions.RacePtsAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).RacePoints) };
        yield return new object[] { SortOptions.RacePtsDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).RacePoints) };
        yield return new object[] { SortOptions.TotalPtsAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).TotalPoints) };
        yield return new object[] { SortOptions.TotalPtsDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).TotalPoints) };
        yield return new object[] { SortOptions.PenPtsAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).PenaltyPoints) };
        yield return new object[] { SortOptions.PenPtsDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).PenaltyPoints) };
        yield return new object[] { SortOptions.IncsAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).Incidents) };
        yield return new object[] { SortOptions.IncsDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).Incidents) };
        yield return new object[] { SortOptions.LastRaceOrderAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).ResultRows.Last().FinalPosition) };
        yield return new object[] { SortOptions.LastRaceOrderDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).ResultRows.Last().FinalPosition) };
        yield return new object[] { SortOptions.WinsAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).Wins) };
        yield return new object[] { SortOptions.WinsDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).Wins) };
        yield return new object[] { SortOptions.Top3Asc, new Func<object, double>(x => ((StandingRowCalculationResult)x).Top3) };
        yield return new object[] { SortOptions.Top3Desc, new Func<object, double>(x => -((StandingRowCalculationResult)x).Top3) };
        yield return new object[] { SortOptions.Top5Asc, new Func<object, double>(x => ((StandingRowCalculationResult)x).Top5) };
        yield return new object[] { SortOptions.Top5Desc, new Func<object, double>(x => -((StandingRowCalculationResult)x).Top5) };
        yield return new object[] { SortOptions.Top10Asc, new Func<object, double>(x => ((StandingRowCalculationResult)x).Top10) };
        yield return new object[] { SortOptions.Top10Desc, new Func<object, double>(x => -((StandingRowCalculationResult)x).Top10) };
        yield return new object[] { SortOptions.RacesAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).Races) };
        yield return new object[] { SortOptions.RacesDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).Races) };
        yield return new object[] { SortOptions.RacesCountedAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).RacesCounted) };
        yield return new object[] { SortOptions.RacesCountedDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).RacesCounted) };
        yield return new object[] { SortOptions.RacesScoredAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).RacesScored) };
        yield return new object[] { SortOptions.RacesScoredDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).RacesScored) };
        yield return new object[] { SortOptions.RacesInPointsAsc, new Func<object, double>(x => ((StandingRowCalculationResult)x).RacesInPoints) };
        yield return new object[] { SortOptions.RacesInPointsDesc, new Func<object, double>(x => -((StandingRowCalculationResult)x).RacesInPoints) };
    }

    [Fact]
    public async Task Calculate_ShouldNotThrow()
    {
        var data = GetCalculationData();
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        var sut = CreateSut(config);

        await sut.Calculate(data);
    }

    [Fact]
    public async Task Calculate_ShouldAccumulateMemberRows()
    {
        const int nEvents = 3;
        const int nRaces = 2;
        var memberId = fixture.Create<long>();
        var testRowData = TestRowBuilder()
            .With(x => x.MemberId, memberId)
            .With(x => x.PointsEligible, true)
            .CreateMany(nEvents * nRaces);
        testRowData.TakeLast(2).ForEach(x => x.PointsEligible = false);
        testRowData.TakeLast(3).ForEach(x => x.RacePoints = x.BonusPoints = 0);
        var data = CalculationDataBuilder(3, 2, true).Create();
        var tmp = data.PreviousEventResults.SelectMany(x => x.SessionResults).Concat(data.CurrentEventResult.SessionResults)
            .Zip(testRowData);
        foreach (var (result, rowData) in tmp)
        {
            result.ResultRows = result.ResultRows.Concat(new[] { rowData });
        }
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId)
            .With(x => x.ResultKind, Common.Enums.ResultKind.Member)
            .With(x => x.UseCombinedResult, false)
            .With(x => x.WeeksCounted, nEvents)
            .Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        test.StandingRows.Should().ContainSingle(x => x.MemberId == memberId);
        var testRow = test.StandingRows.Single(x => x.MemberId == memberId);
        var lastResultRowData = testRowData.TakeLast(2);
        testRow.Wins.Should().Be(testRowData.Sum(x => x.FinalPosition == 1 ? 1 : 0));
        testRow.RacePoints.Should().Be((int)testRowData.Sum(x => x.RacePoints + x.BonusPoints));
        testRow.TotalPoints.Should().Be((int)testRowData.Sum(x => x.RacePoints + x.BonusPoints - x.PenaltyPoints));
        testRow.CompletedLaps.Should().Be((int)testRowData.Sum(x => x.CompletedLaps));
        testRow.CompletedLapsChange.Should().Be((int)lastResultRowData.Sum(x => x.CompletedLaps));
        testRow.DroppedResultCount.Should().Be(0);
        testRow.Incidents.Should().Be((int)testRowData.Sum(x => x.Incidents));
        testRow.IncidentsChange.Should().Be((int)lastResultRowData.Sum(x => x.Incidents));
        testRow.LeadLaps.Should().Be((int)testRowData.Sum(x => x.LeadLaps));
        testRow.LeadLapsChange.Should().Be((int)lastResultRowData.Sum(x => x.LeadLaps));
        testRow.PenaltyPoints.Should().Be((int)testRowData.Sum(x => x.PenaltyPoints));
        testRow.PenaltyPointsChange.Should().Be((int)lastResultRowData.Sum(x => x.PenaltyPoints));
        testRow.PolePositions.Should().Be(testRowData.Count(x => x.StartPosition == 1));
        testRow.PolePositionsChange.Should().Be(lastResultRowData.Count(x => x.StartPosition == 1));
        testRow.Races.Should().Be(testRowData.Count());
        testRow.RacesCounted.Should().Be(testRowData.Count());
        testRow.Top10.Should().Be(testRowData.Count(x => x.FinalPosition <= 10));
        testRow.Top5.Should().Be(testRowData.Count(x => x.FinalPosition <= 5));
        testRow.Top3.Should().Be(testRowData.Count(x => x.FinalPosition <= 3));
        testRow.RacesScored.Should().Be(testRowData.Count() - 2);
        testRow.RacesInPoints.Should().Be(testRowData.Count() - 3);
    }

    [Fact]
    public async Task Calculate_ShouldOnlyUseLastSessionResult_WhenCombinedResultConfigure_AndCombinedResultExists()
    {
        const int nEvents = 3;
        const int nRaces = 2;
        var memberId = fixture.Create<long>();
        var testRowData = TestRowBuilder()
            .With(x => x.MemberId, memberId)
            .CreateMany(nEvents * (nRaces + 1));
        var data = CalculationDataBuilder(nEvents, nRaces + 1, false).Create();
        data.PreviousEventResults.ForEach(x => x.SessionResults.Last().SessionNr = 999);
        data.CurrentEventResult.SessionResults.Last().SessionNr = 999;
        var tmp = data.PreviousEventResults.SelectMany(x => x.SessionResults.OrderBy(x => x.SessionNr)).Concat(data.CurrentEventResult.SessionResults.OrderBy(x => x.SessionNr))
            .Zip(testRowData);
        foreach (var (result, rowData) in tmp)
        {
            result.ResultRows = [.. result.ResultRows, rowData];
        }
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId)
            .With(x => x.ResultKind, ResultKind.Member)
            .With(x => x.UseCombinedResult, true)
            .With(x => x.WeeksCounted, nEvents)
            .Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        test.StandingRows.Should().ContainSingle(x => x.MemberId == memberId);
        var testRow = test.StandingRows.Single(x => x.MemberId == memberId);
        var combinedResultRowData = testRowData.Chunk(3).Select(x => x.Last()).ToList();
        var lastResultRowData = combinedResultRowData.TakeLast(1);
        testRow.Wins.Should().Be(combinedResultRowData.Sum(x => x.FinalPosition == 1 ? 1 : 0));
        testRow.RacePoints.Should().Be((int)combinedResultRowData.Sum(x => x.RacePoints + x.BonusPoints));
        testRow.TotalPoints.Should().Be((int)combinedResultRowData.Sum(x => x.RacePoints + x.BonusPoints - x.PenaltyPoints));
        testRow.CompletedLaps.Should().Be((int)combinedResultRowData.Sum(x => x.CompletedLaps));
        testRow.CompletedLapsChange.Should().Be((int)lastResultRowData.Sum(x => x.CompletedLaps));
        testRow.DroppedResultCount.Should().Be(0);
        testRow.Incidents.Should().Be((int)combinedResultRowData.Sum(x => x.Incidents));
        testRow.IncidentsChange.Should().Be((int)lastResultRowData.Sum(x => x.Incidents));
        testRow.LeadLaps.Should().Be((int)combinedResultRowData.Sum(x => x.LeadLaps));
        testRow.LeadLapsChange.Should().Be((int)lastResultRowData.Sum(x => x.LeadLaps));
        testRow.PenaltyPoints.Should().Be((int)combinedResultRowData.Sum(x => x.PenaltyPoints));
        testRow.PenaltyPointsChange.Should().Be((int)lastResultRowData.Sum(x => x.PenaltyPoints));
        testRow.PolePositions.Should().Be(combinedResultRowData.Count(x => x.StartPosition == 1));
        testRow.PolePositionsChange.Should().Be(lastResultRowData.Count(x => x.StartPosition == 1));
        testRow.Races.Should().Be(combinedResultRowData.Count());
        testRow.RacesCounted.Should().Be(combinedResultRowData.Count());
        testRow.Top10.Should().Be(combinedResultRowData.Count(x => x.FinalPosition <= 10));
        testRow.Top5.Should().Be(combinedResultRowData.Count(x => x.FinalPosition <= 5));
        testRow.Top3.Should().Be(combinedResultRowData.Count(x => x.FinalPosition <= 3));
    }

    [Fact]
    public async Task Calculate_ShouldAssignPositions_ByTotalPointsAndPenaltyPoints()
    {
        var nEvents = 3;
        var nRaces = 1;
        var data = CalculationDataBuilder(nEvents, nRaces, false).Create();
        // make sure two rows have same total points
        var racePoints = fixture.Create<double>();
        var bonusPoints = fixture.Create<double>();
        var penaltyPoints = fixture.Create<double>();
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(0).RacePoints = racePoints;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(1).RacePoints = racePoints;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(0).BonusPoints = bonusPoints;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(1).BonusPoints = bonusPoints + penaltyPoints;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(0).PenaltyPoints = 0.0;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(1).PenaltyPoints = penaltyPoints;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(0).TotalPoints = racePoints + bonusPoints;
        data.CurrentEventResult.SessionResults.First().ResultRows.ElementAt(1).TotalPoints = racePoints + bonusPoints - penaltyPoints;
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId)
            .With(x => x.ResultKind, ResultKind.Member)
            .With(x => x.WeeksCounted, nEvents)
            .With(x => x.SortOptions, new[]
            {
                SortOptions.TotalPtsDesc,
                SortOptions.PenPtsAsc,
            })
            .Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        var testOrder = test.StandingRows
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.PenaltyPoints);
        var positions = Enumerable.Range(1, testOrder.Count());
        testOrder.Should().BeInAscendingOrder(x => x.Position);
        testOrder.Select(x => x.Position).Should().BeEquivalentTo(positions);
        test.StandingRows.Should().BeEquivalentTo(testOrder);
    }

    [Theory]
    [MemberData(nameof(TestSortOptions))]
    public async Task Calculate_ShouldAssignPositions_BySortOption(SortOptions sortOption, Func<object, double> testSortFunc)
    {
        var nEvents = 3;
        var nRaces = 1;
        var data = CalculationDataBuilder(nEvents, nRaces, false).Create();
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId)
            .With(x => x.ResultKind, ResultKind.Member)
            .With(x => x.WeeksCounted, nEvents)
            .With(x => x.SortOptions, new[]
            {
                sortOption,
            })
            .Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        var testOrder = test.StandingRows
            .OrderBy(x => testSortFunc(x));
        var positions = Enumerable.Range(1, testOrder.Count());
        testOrder.Should().BeInAscendingOrder(x => x.Position);
        testOrder.Select(x => x.Position).Should().BeEquivalentTo(positions);
        test.StandingRows.Should().BeEquivalentTo(testOrder);
    }

    [Fact]
    public async Task Calculate_ShouldSetChampSeasonId()
    {
        var data = GetCalculationData();
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        test.ChampSeasonId.Should().Be(config.ChampSeasonId);
    }

    [Fact]
    public async Task Calculate_ShouldConsiderDropweeks()
    {
        const int nEvents = 5;
        const int nRaces = 1;
        var points = ((double[])[8, 1, 4, 0, 2])
            .CreateSequence();
        var memberId = fixture.Create<long>();
        var testRowData = TestRowBuilder()
            .With(x => x.MemberId, memberId)
            .With(x => x.PointsEligible, true)
            .With(x => x.RacePoints, points)
            .With(x => x.BonusPoints, 0)
            .CreateMany(nEvents * nRaces);
        var data = CalculationDataBuilder(nEvents, nRaces, false).Create();
        var tmp = data.PreviousEventResults.SelectMany(x => x.SessionResults).Concat(data.CurrentEventResult.SessionResults)
            .Zip(testRowData);
        foreach (var (result, rowData) in tmp)
        {
            result.ResultRows = result.ResultRows.Concat(new[] { rowData });
        }
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId)
            .With(x => x.WeeksCounted, 3)
            .Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        var testRow = test.StandingRows.Single(x => x.MemberId == memberId);
        testRow.RacePoints.Should().Be(14);
        testRow.RacePointsChange.Should().Be(1);
    }

    //[Fact]
    //public async Task Calculate_ShouldConsiderDropweekOverrides()
    //{
    //    const int nEvents = 5;
    //    const int nRaces = 1;
    //    var points = ((double[])[8, 1, 4, 0, 2])
    //        .CreateSequence();
    //    var memberId = fixture.Create<long>();
    //    var testRowData = TestRowBuilder()
    //        .With(x => x.MemberId, memberId)
    //        .With(x => x.PointsEligible, true)
    //        .With(x => x.RacePoints, points)
    //        .With(x => x.BonusPoints, 0)
    //        .CreateMany(nEvents * nRaces);
    //    var keepRace = testRowData.ElementAt(1);
    //    var dropRace = testRowData.ElementAt(2);
    //    dropRace.DropweekOverride = new()
    //    {
    //        ScoredResultRowId = dropRace.ScoredResultRowId!.Value,
    //        ShouldDrop = true,
    //    };
    //    keepRace.DropweekOverride = new()
    //    {
    //        ScoredResultRowId = keepRace.ScoredResultRowId!.Value,
    //        ShouldDrop = false,
    //    };
    //    var data = CalculationDataBuilder(nEvents, nRaces, false).Create();
    //    var tmp = data.PreviousEventResults.SelectMany(x => x.SessionResults).Concat(data.CurrentEventResult.SessionResults)
    //        .Zip(testRowData);
    //    foreach (var (result, rowData) in tmp)
    //    {
    //        result.ResultRows = [.. result.ResultRows, rowData];
    //    }
    //    var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId)
    //        .With(x => x.WeeksCounted, 3)
    //        .Create();
    //    var sut = CreateSut(config);

    //    var test = await sut.Calculate(data);

    //    var testRow = test.StandingRows.Single(x => x.MemberId == memberId);
    //    testRow.RacePoints.Should().Be(11);
    //    testRow.RacePointsChange.Should().Be(2);
    //}

    [Fact]
    public async Task Calculate_ShouldCombinePracticeAndQualyPoints_ForSingleHeader()
    {
        var nEvents = 1;
        var nRaces = 1;
        var data = CalculationDataBuilder(nEvents, nRaces, false).Create();
        var memberIds = data.CurrentEventResult.SessionResults.First().ResultRows.Select(x => x.MemberId).NotNull().Distinct().ToList();
        // add qualy and practice sessions
        var practiceSession = SessionResultDataBuilder(memberIds, SessionType.Practice).Create();
        var qualySession = SessionResultDataBuilder(memberIds, SessionType.Qualifying).Create();
        data.CurrentEventResult.SessionResults = [practiceSession, qualySession, .. data.CurrentEventResult.SessionResults];
        // add points to qualy and race session test row
        var practiceTestRow = practiceSession.ResultRows.First();
        practiceTestRow.RacePoints = 1;
        practiceTestRow.BonusPoints = 2;
        practiceTestRow.PenaltyPoints = 4;
        var qualyTestRow = qualySession.ResultRows.First(x => x.MemberId == practiceTestRow.MemberId);
        qualyTestRow.RacePoints = 8;
        qualyTestRow.BonusPoints = 16;
        qualyTestRow.PenaltyPoints = 32;
        var raceTestRow = data.CurrentEventResult.SessionResults.Last().ResultRows.First(x => x.MemberId == practiceTestRow.MemberId);
        raceTestRow.RacePoints = 0;
        raceTestRow.BonusPoints = 0;
        raceTestRow.PenaltyPoints = 0;
        raceTestRow.TotalPoints = 0;

        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        var testRow = test.StandingRows.First(x => x.MemberId == practiceTestRow.MemberId);
        testRow.RacePoints.Should().Be(27);
        testRow.PenaltyPoints.Should().Be(36);
        testRow.TotalPoints.Should().Be(-9);
    }

    [Fact]
    public async Task Calculate_ShouldCombinePracticeAndQualyPoints_ForMultiHeader()
    {
        var nEvents = 1;
        var nRaces = 3;
        var data = CalculationDataBuilder(nEvents, nRaces, false).Create();
        var memberIds = data.CurrentEventResult.SessionResults.First().ResultRows.Select(x => x.MemberId).NotNull().Distinct().ToList();
        // add qualy and practice sessions
        var practiceSession = SessionResultDataBuilder(memberIds, SessionType.Practice).Create();
        var qualySession = SessionResultDataBuilder(memberIds, SessionType.Qualifying).Create();
        data.CurrentEventResult.SessionResults = [practiceSession, qualySession, .. data.CurrentEventResult.SessionResults];
        // add points to qualy and race session test row
        var practiceTestRow = practiceSession.ResultRows.First();
        practiceTestRow.RacePoints = 1;
        practiceTestRow.BonusPoints = 2;
        practiceTestRow.PenaltyPoints = 4;
        var qualyTestRow = qualySession.ResultRows.First(x => x.MemberId == practiceTestRow.MemberId);
        qualyTestRow.RacePoints = 8;
        qualyTestRow.BonusPoints = 16;
        qualyTestRow.PenaltyPoints = 32;
        data.CurrentEventResult.SessionResults
            .Where(x => x.SessionType == SessionType.Race)
            .ForEach(x =>
            {
                var row = x.ResultRows.First(x => x.MemberId == practiceTestRow.MemberId);
                row.RacePoints = 0;
                row.BonusPoints = 0;
                row.PenaltyPoints = 0;
                row.TotalPoints = 0;
            });

        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);

        var testRow = test.StandingRows.First(x => x.MemberId == practiceTestRow.MemberId);
        testRow.RacePoints.Should().Be(27);
        testRow.PenaltyPoints.Should().Be(36);
        testRow.TotalPoints.Should().Be(-9);
    }

    [Fact]
    public async Task Calculate_ShouldSetPositionChangeZero_WhenFirstEvent()
    {
        var nEvents = 1;
        var nRaces = 3;
        var data = CalculationDataBuilder(nEvents, nRaces, false).Create();

        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        config.SortOptions = [SortOptions.TotalPtsDesc];
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);
        test.StandingRows.Should().AllSatisfy(x => x.PositionChange.Should().Be(0));
    }

    [Fact]
    public async Task Calculate_ShouldSetPositionChange_WhenDriverMissingInLastEvent()
    {
        var nEvents = 3;
        var nRaces = 1;
        var memberIds = fixture.CreateMany<long>(3);
        var testMemberId = memberIds.ElementAt(1);
        var data = CalculationDataBuilder(nEvents, nRaces, false)
            .With(x => x.PreviousEventResults, () => EventResultDataBuilder(memberIds, nRaces: nRaces, randomMemberOrder: false).CreateMany(nEvents - 1).ToList())
            .With(x => x.CurrentEventResult, () => EventResultDataBuilder(memberIds.Except([testMemberId]), nRaces: nRaces, randomMemberOrder: false).Create())
            .Create();

        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        var sut = CreateSut(config);

        var test = await sut.Calculate(data);
        var testRow = test.StandingRows.First(x => x.MemberId == testMemberId);
        testRow.PositionChange.Should().Be(-1);
    }

    private static MemberStandingCalculationService CreateSut(StandingCalculationConfiguration config)
    {
        return new(config);
    }

    private StandingCalculationData GetCalculationData()
    {
        return CalculationDataBuilder().Create();
    }

    private IPostprocessComposer<StandingCalculationData> CalculationDataBuilder(int nEvents = 3, int nRacesPerEvent = 3, bool hasCombinedResult = false,
        bool randomMemberOrder = true)
    {
        var memberIds = fixture.CreateMany<long>(10);
        return fixture.Build<StandingCalculationData>()
            .With(x => x.PreviousEventResults, () => EventResultDataBuilder(memberIds, nRacesPerEvent, hasCombinedResult: hasCombinedResult,
                randomMemberOrder: randomMemberOrder).CreateMany(nEvents - 1).ToList())
            .With(x => x.CurrentEventResult, () => EventResultDataBuilder(memberIds, nRacesPerEvent, hasCombinedResult: hasCombinedResult,
                randomMemberOrder: randomMemberOrder).Create());
    }

    private IPostprocessComposer<EventCalculationResult> EventResultDataBuilder(IEnumerable<long> memberIds, int nRaces = 2, bool hasCombinedResult = false,
        bool randomMemberOrder = true)
    {
        return fixture.Build<EventCalculationResult>()
            .With(x => x.SessionResults, () => SessionResultDataBuilder(memberIds, randomMemberOrder: randomMemberOrder).CreateMany(nRaces));
    }

    private IPostprocessComposer<SessionCalculationResult> SessionResultDataBuilder(IEnumerable<long> memberIds, SessionType sessionType = SessionType.Race,
        bool randomMemberOrder = true)
    {
        return fixture
            .Build<SessionCalculationResult>()
            .With(x => x.SessionType, sessionType)
            .With(x => x.SessionNr, fixture.Create<int>() % 10 + 1)
            .With(x => x.ResultRows, () =>
                {
                    var getMemberIds = memberIds.ToList();
                    return fixture.Build<ResultRowCalculationResult>()
                        .With(x => x.MemberId, () => randomMemberOrder ? getMemberIds.PopRandom() : getMemberIds.PopFirst())
                        .With(x => x.SessionType, sessionType)
                        .CreateMany(memberIds.Count());
                }
            );
    }

    private IPostprocessComposer<StandingCalculationConfiguration> CalculationConfigurationBuilder(long leagueId, long eventId,
        ChampSeasonEntity? champSeason = null)
    {
        return fixture.Build<StandingCalculationConfiguration>()
            .With(x => x.LeagueId, leagueId)
            .With(x => x.EventId, eventId)
            .With(x => x.ResultConfigs, champSeason?.ResultConfigurations.Select(x => x.ResultConfigId) ?? Array.Empty<long>());
    }

    private IPostprocessComposer<ResultRowCalculationResult> TestRowBuilder()
    {
        return fixture.Build<ResultRowCalculationResult>()
            .Without(x => x.AddPenalties)
            .With(x => x.SessionType, SessionType.Race)
            .Do(x => { x.TotalPoints = x.RacePoints + x.BonusPoints - x.PenaltyPoints; });
    }

    private IEnumerable<ResultRowCalculationData> GetTestRows()
    {
        return TestRowBuilder().CreateMany();
    }
}
