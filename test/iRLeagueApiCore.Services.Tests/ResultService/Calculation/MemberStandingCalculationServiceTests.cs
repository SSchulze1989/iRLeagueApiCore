using AutoFixture.Dsl;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation;

public sealed class MemberStandingCalculationServiceTests
{
    private readonly Fixture fixture = new();

    [Fact]
    public async Task Calculate_ShouldNotThrow()
    {
        var data = GetCalculationData();
        var config = CalculationConfigurationBuilder(data.LeagueId, data.EventId).Create();
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);
    }

    [Fact]
    public async Task Calculate_ShouldAccumulateSingleMemberRows()
    {
        const int nEvents = 3;
        const int nRaces = 2;
        var memberId = fixture.Create<long>();
        var testRowData = TestRowBuilder()
            .With(x => x.MemberId, memberId)
            .CreateMany(nEvents * nRaces);
        var data = CalculationDataBuilder(3, 2, false).Create();
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
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);

        test.StandingRows.Should().ContainSingle(x => x.MemberId == memberId);
        var testRow = test.StandingRows.Single(x => x.MemberId == memberId);
        testRow.Wins.Should().Be(testRowData.Sum(x => x.FinalPosition == 1 ? 1 : 0));
        testRow.RacePoints.Should().Be((int)testRowData.Sum(x => x.RacePoints));
        testRow.CompletedLaps.Should().Be((int)testRowData.Sum(x => x.CompletedLaps));
    }

    private MemberStandingCalculationService CreateSut()
    {
        return fixture.Create<MemberStandingCalculationService>();
    }

    private StandingCalculationData GetCalculationData()
    {
        return fixture.Create<StandingCalculationData>();
    }

    private IPostprocessComposer<StandingCalculationData> CalculationDataBuilder(int nEvents = 3, int nRacesPerEvent = 3, bool hasCombinedResult = false)
    {
        return fixture.Build<StandingCalculationData>()
            .With(x => x.PreviousEventResults, () => EventResultDataBuilder(nRacesPerEvent, hasCombinedResult).CreateMany(nEvents - 1).ToList())
            .With(x => x.CurrentEventResult, () => EventResultDataBuilder(nRacesPerEvent, hasCombinedResult).Create());
    }

    private IPostprocessComposer<EventCalculationResult> EventResultDataBuilder(int nRaces = 2, bool hasCombinedResult = false)
    {
        return fixture.Build<EventCalculationResult>()
            .With(x => x.SessionResults, () => SessionResultDataBuilder().CreateMany(nRaces));
    }

    private IPostprocessComposer<SessionCalculationResult> SessionResultDataBuilder()
    {
        return fixture.Build<SessionCalculationResult>();
    }

    private IPostprocessComposer<StandingCalculationConfiguration> CalculationConfigurationBuilder(long leagueId, long eventId, long? resultConfigId = null)
    {
        return fixture.Build<StandingCalculationConfiguration>()
            .With(x => x.LeagueId, leagueId)
            .With(x => x.EventId, eventId)
            .With(x => x.ResultConfigId, resultConfigId);
    }

    private IPostprocessComposer<ResultRowCalculationResult> TestRowBuilder()
    {
        return fixture.Build<ResultRowCalculationResult>()
            .Without(x => x.AddPenalty)
            .Do(x => x.TotalPoints = x.RacePoints + x.BonusPoints - x.PenaltyPoints);
    }

    private IEnumerable<ResultRowCalculationData> GetTestRows()
    {
        return TestRowBuilder().CreateMany();
    }
}
