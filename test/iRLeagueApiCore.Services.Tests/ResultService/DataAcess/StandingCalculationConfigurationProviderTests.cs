using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess;

[Collection("DataAccessTests")]
public sealed class StandingCalculationConfigurationProviderTests : DataAccessTestsBase
{
    [Theory]
    [InlineData(default(long))]
    [InlineData(42)]
    public async Task GetConfiguration_ShouldProvideEmptyConfiguration_WhenEventDoesNotExist(long? eventId)
    {
        var season = await GetFirstSeasonAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(season.SeasonId, eventId, null);

        test.LeagueId.Should().Be(0);
        test.SeasonId.Should().Be(0);
        test.EventId.Should().Be(0);
        test.ResultConfigId.Should().BeNull();
    }

    [Fact]
    public async Task GetConfiguration_ShouldConfigurationForEvent_WhenEventIdIsNotNull()
    {
        var season = await GetFirstSeasonAsync();
        var @event = season.Schedules.First().Events.First();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(season.SeasonId, @event.EventId, null);

        test.LeagueId.Should().Be(season.LeagueId);
        test.SeasonId.Should().Be(season.SeasonId);
        test.EventId.Should().Be(@event.EventId);
        test.ResultConfigId.Should().BeNull();
    }

    [Fact]
    public async Task GetConfiguration_ShouldProvideConfigurationForLatestEvent_WhenEventIdIsNull()
    {
        var season = await GetFirstSeasonAsync();
        // create results for first two events
        var events = season.Schedules.SelectMany(x => x.Events).OrderBy(x => x.Date);
        foreach (var @event in events.Take(2))
        {
            var result = accessMockHelper.CreateScoredResult(@event, null);
            @event.ScoredEventResults.Add(result);
            dbContext.ScoredEventResults.Add(result);
        }
        var latestEvent = events.ElementAt(1);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(season.SeasonId, null, null);

        test.LeagueId.Should().Be(season.LeagueId);
        test.SeasonId.Should().Be(season.SeasonId);
        test.EventId.Should().Be(latestEvent.EventId);
        test.ResultConfigId.Should().BeNull();
    }

    [Fact]
    public async Task GetConfiguration_ShouldProvideConfiguration_WithStandingConfigId()
    {
        var season = await GetFirstSeasonAsync();
        var @event = season.Schedules.First().Events.First();
        var config = accessMockHelper.CreateConfiguration(@event);
        var standingConfig = accessMockHelper.CreateStandingConfiguration(config);
        config.StandingConfigurations.Add(standingConfig);
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(season.SeasonId, @event.EventId, standingConfig.StandingConfigId);

        test.LeagueId.Should().Be(season.LeagueId);
        test.SeasonId.Should().Be(season.SeasonId);
        test.EventId.Should().Be(@event.EventId);
        test.ResultConfigId.Should().Be(config.ResultConfigId);
        test.StandingConfigId.Should().Be(standingConfig.StandingConfigId);
    }

    private StandingCalculationConfigurationProvider CreateSut()
    {
        return fixture.Create<StandingCalculationConfigurationProvider>();
    }

    private async Task<SeasonEntity> GetFirstSeasonAsync()
    {
        return await dbContext.Seasons
            .Include(x => x.Schedules)
                .ThenInclude(x => x.Events)
                    .ThenInclude(x => x.ScoredEventResults)
            .FirstAsync();
    }
}
