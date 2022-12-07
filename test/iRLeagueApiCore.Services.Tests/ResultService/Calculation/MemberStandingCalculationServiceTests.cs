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
        var config = GetCalculationConfiguration(data.LeagueId, data.EventId);
        fixture.Register(() => config);
        var sut = CreateSut();

        var test = await sut.Calculate(data);
    }

    private MemberStandingCalculationService CreateSut()
    {
        return fixture.Create<MemberStandingCalculationService>();
    }

    private StandingCalculationData GetCalculationData()
    {
        return fixture.Create<StandingCalculationData>();
    }

    private StandingCalculationConfiguration GetCalculationConfiguration(long leagueId, long eventId, long? resultConfigId = null)
    {
        return fixture.Build<StandingCalculationConfiguration>()
            .With(x => x.LeagueId, leagueId)
            .With(x => x.EventId, eventId)
            .With(x => x.ResultConfigId, resultConfigId)
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
