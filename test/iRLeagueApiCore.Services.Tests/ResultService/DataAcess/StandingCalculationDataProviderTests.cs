﻿using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess;

[Collection("DataAccessTests")]
public sealed class StandingCalculationDataProviderTests : DataAccessTestsBase
{
    [Fact]
    public async Task GetData_ShouldProvidePreviousResults()
    {
        var prevCount = 2;
        var season = await GetFirstSeasonAsync();
        var events = season.Schedules.SelectMany(x => x.Events).OrderBy(x => x.Date);
        AddMultipleScoredEventResults(events, null, prevCount + 1);
        await dbContext.SaveChangesAsync();
        var config = CreateStandingConfiguration(season, events.ElementAt(prevCount), null);
        var sut = CreateSut();

        var test = await sut.GetData(config);

        test.Should().NotBeNull();
        test!.LeagueId.Should().Be(season.LeagueId);
        test.SeasonId.Should().Be(season.SeasonId);
        test.PreviousEventResults.Should().HaveCount(prevCount);
        foreach(var (result, prevEvent) in test.PreviousEventResults.Zip(events.Take(prevCount)))
        {
            result.EventId.Should().Be(prevEvent.EventId);
            result.SessionResults.Should().HaveCountGreaterThanOrEqualTo(prevEvent.Sessions.Count);
        }
    }

    [Fact]
    public async Task GetData_ShouldProvideCurrentResult()
    {
        var prevCount = 2;
        var season = await GetFirstSeasonAsync();
        var events = season.Schedules.SelectMany(x => x.Events).OrderBy(x => x.Date);
        AddMultipleScoredEventResults(events, null, prevCount + 1);
        await dbContext.SaveChangesAsync();
        var @event = events.ElementAt(prevCount);
        var config = CreateStandingConfiguration(season, @event, null);
        var sut = CreateSut();

        var test = await sut.GetData(config);

        test.Should().NotBeNull();
        test!.EventId.Should().Be(@event.EventId);
        var result = test.CurrentEventResult;
        result.EventId.Should().Be(@event.EventId);
        result.SessionResults.Should().HaveCountGreaterThanOrEqualTo(@event.Sessions.Count);
    }

    private async Task<SeasonEntity> GetFirstSeasonAsync()
    {
        return await dbContext.Seasons
            .Include(x => x.Schedules)
                .ThenInclude(x => x.Events)
                    .ThenInclude(x => x.ScoredEventResults)
            .FirstAsync();
    }

    private void AddMultipleScoredEventResults(IEnumerable<EventEntity> events, ResultConfigurationEntity? config, int count)
    {
        foreach (var @event in events.Take(count))
        {
            var result = accessMockHelper.CreateScoredResult(@event, config);
            @event.ScoredEventResults.Add(result);
            dbContext.ScoredEventResults.Add(result);
        }
    }

    private StandingCalculationConfiguration CreateStandingConfiguration(SeasonEntity season, EventEntity @event, ResultConfigurationEntity? config)
    {
        return fixture.Build<StandingCalculationConfiguration>()
            .With(x => x.LeagueId, season.LeagueId)
            .With(x => x.SeasonId, season.SeasonId)
            .With(x => x.EventId, @event.EventId)
            .With(x => x.ResultConfigId, config?.ResultConfigId)
            .With(x => x.WeeksCounted, 2)
            .Create();
    }

    private StandingCalculationDataProvider CreateSut()
    {
        return fixture.Create<StandingCalculationDataProvider>();
    }
}
