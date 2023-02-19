using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAccess;

[Collection("DataAccessTests")]
public sealed class SessionCalculationConfigurationProviderTests : DataAccessTestsBase
{
    [Fact]
    public async Task GetConfigurations_ShouldProvideDefaultConfiguration_WhenConfigIsNull()
    {
        var @event = await GetFirstEventEntity();
        var config = (ResultConfigurationEntity?)null;
        var sut = fixture.Create<SessionCalculationConfigurationProvider>();

        var test = await sut.GetConfigurations(@event, config);

        test.Should().HaveSameCount(@event.Sessions);
        foreach ((var sessionConfig, var session) in test.Zip(@event.Sessions.OrderBy(x => x.SessionNr)))
        {
            sessionConfig.LeagueId.Should().Be(@event.LeagueId);
            sessionConfig.SessionId.Should().Be(session.SessionId);
            sessionConfig.ScoringId.Should().BeNull();
            sessionConfig.UpdateTeamOnRecalculation.Should().BeFalse();
            sessionConfig.UseResultSetTeam.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideDefaultSessionResultId_WhenCalculatedResultExistsAndConfigIsNull()
    {
        var @event = await GetFirstEventEntity();
        var config = (ResultConfigurationEntity?)null;
        var scoredEventResult = accessMockHelper.CreateScoredResult(@event, config);
        dbContext.ScoredEventResults.Add(scoredEventResult);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        test.Should().HaveSameCount(scoredEventResult.ScoredSessionResults);
        foreach ((var sessionConfig, var sessionResult) in test.Zip(scoredEventResult.ScoredSessionResults.OrderBy(x => x.SessionNr)))
        {
            sessionConfig.SessionResultId.Should().Be(sessionResult.SessionResultId);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideConfigurationFromEntity_WhenConfigIsNotNull()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = fixture.Create<SessionCalculationConfigurationProvider>();

        var test = await sut.GetConfigurations(@event, config);

        test.Should().HaveSameCount(@event.Sessions);
        test.Should().HaveSameCount(config.Scorings);
        foreach ((var sessionConfig, var session, var scoring) in test.Zip(@event.Sessions.OrderBy(x => x.SessionNr), config.Scorings.OrderBy(x => x.Index)))
        {
            sessionConfig.LeagueId.Should().Be(@event.LeagueId);
            sessionConfig.SessionId.Should().Be(session.SessionId);
            sessionConfig.ScoringId.Should().Be(scoring.ScoringId);
            sessionConfig.MaxResultsPerGroup.Should().Be(config.ResultsPerTeam);
            sessionConfig.UpdateTeamOnRecalculation.Should().Be(scoring.UpdateTeamOnRecalculation);
            sessionConfig.UseResultSetTeam.Should().Be(scoring.UseResultSetTeam);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideMemberKindConfig_WhenResultConfigIsMemberKind()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        config.ResultKind = ResultKind.Member;
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = fixture.Create<SessionCalculationConfigurationProvider>();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            sessionConfig.ResultKind.Should().Be(ResultKind.Member);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideTeamKindConfig_WhenResultConfigIsTeamKind()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        config.ResultKind = ResultKind.Team;
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = fixture.Create<SessionCalculationConfigurationProvider>();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            sessionConfig.ResultKind.Should().Be(ResultKind.Team);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideConfigurationSessionResultId_WhenCalculatedResultExistsAndConfigIsNotNull()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        ScoredEventResultEntity scoredEventResult = accessMockHelper.CreateScoredResult(@event, config);
        dbContext.ScoredEventResults.Add(scoredEventResult);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        test.Should().HaveSameCount(scoredEventResult.ScoredSessionResults);
        foreach ((var sessionConfig, var sessionResult) in test.Zip(scoredEventResult.ScoredSessionResults.OrderBy(x => x.SessionNr)))
        {
            sessionConfig.SessionResultId.Should().Be(sessionResult.SessionResultId);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideUseResultPointsPointRule_WhenScoringOrPointRuleEntityIsNull()
    {
        var @event = await GetFirstEventEntity();
        // creates default config with null point rule
        var config = accessMockHelper.CreateConfiguration(@event);
        // for testing null scoring
        config.Scorings.Remove(config.Scorings.Last());
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            sessionConfig.PointRule.Should().BeOfType<UseResultPointsPointRule>();
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideDefaultConfiguration_WhenSessionIsPracticeOrQualifying()
    {
        var @event = await GetFirstEventEntity();
        var practice = accessMockHelper.CreateSession(@event, SessionType.Practice);
        var qualy = accessMockHelper.CreateSession(@event, SessionType.Qualifying);
        var sessionNr = 1;
        practice.SessionNr = sessionNr++;
        qualy.SessionNr = sessionNr++;
        @event.Sessions.ForEeach(x => x.SessionNr = sessionNr++);
        var config = accessMockHelper.CreateConfiguration(@event);
        @event.Sessions.Add(practice);
        @event.Sessions.Add(qualy);
        dbContext.Sessions.Add(practice);
        dbContext.Sessions.Add(qualy);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        test.Should().HaveSameCount(@event.Sessions);
        var scoringIndex = 0;
        foreach ((var sessionConfig, var session) in test.Zip(@event.Sessions.OrderBy(x => x.SessionNr)))
        {
            if (session == practice || session == qualy)
            {
                sessionConfig.ScoringId.Should().BeNull();
                sessionConfig.ResultKind.Should().Be(ResultKind.Member);
                sessionConfig.PointRule.Should().BeOfType<UseResultPointsPointRule>();
            }
            else
            {
                sessionConfig.ScoringId.Should().Be(config.Scorings.ElementAt(scoringIndex++).ScoringId);
            }
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideUseResultPointsPointRule_WhenResultPointRuleHasNoPoints()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
        pointRule.PointsPerPlace = new List<int>();
        pointRule.MaxPoints = 0;
        pointRule.PointDropOff = 0;
        dbContext.PointRules.Add(pointRule);
        dbContext.ResultConfigurations.Add(config);
        config.Scorings.ForEeach(x => { x.PointsRule = pointRule; });
        await dbContext.SaveChangesAsync();
        config = await dbContext.ResultConfigurations
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
            .FirstAsync(x => x.ResultConfigId == config.ResultConfigId);
        var scorings = await dbContext.Scorings
            .Include(x => x.ResultConfiguration)
            .ToListAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            sessionConfig.PointRule.Should().BeOfType<UseResultPointsPointRule>();
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideMaxPointRule_WhenNoPointsPerNotPlaceConfigured()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
        pointRule.PointsPerPlace = new List<int>();
        dbContext.PointRules.Add(pointRule);
        dbContext.ResultConfigurations.Add(config);
        config.Scorings.ForEeach(x => { x.PointsRule = pointRule; });
        await dbContext.SaveChangesAsync();
        config = await dbContext.ResultConfigurations
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
            .FirstAsync(x => x.ResultConfigId == config.ResultConfigId);
        var scorings = await dbContext.Scorings
            .Include(x => x.ResultConfiguration)
            .ToListAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            sessionConfig.PointRule.Should().BeOfType(typeof(MaxPointRule));
            var sessionConfigPointRule = (MaxPointRule)sessionConfig.PointRule;
            sessionConfigPointRule.MaxPoints.Should().Be(pointRule.MaxPoints);
            sessionConfigPointRule.DropOff.Should().Be(pointRule.PointDropOff);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvidePerPlacePointRule_WhenPointsPerPlaceConfigured()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
        pointRule.PointsPerPlace = new[] { 3, 2, 1 }.ToList();
        dbContext.PointRules.Add(pointRule);
        dbContext.ResultConfigurations.Add(config);
        config.Scorings.ForEeach(x => { x.PointsRule = pointRule; });
        await dbContext.SaveChangesAsync();
        config = await dbContext.ResultConfigurations
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
            .FirstAsync(x => x.ResultConfigId == config.ResultConfigId);
        var scorings = await dbContext.Scorings
            .Include(x => x.ResultConfiguration)
            .ToListAsync();
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            sessionConfig.PointRule.Should().BeOfType(typeof(PerPlacePointRule));
            var sessionConfigPointRule = (PerPlacePointRule)sessionConfig.PointRule;
            sessionConfigPointRule.PointsPerPlace.Values.Should().BeEquivalentTo(pointRule.PointsPerPlace);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvidePointRuleWithPointFilters_WhenResultConfigHasPointFilters()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
        var condition = fixture.Build<FilterConditionEntity>()
                    .With(x => x.ColumnPropertyName, nameof(ResultRowCalculationResult.Firstname))
                    .Without(x => x.FilterOption)
                    .Create();
        var filter = fixture.Build<FilterOptionEntity>()
            .With(x => x.Conditions, new[]
            {
                    condition,
            })
            .Without(x => x.PointFilterResultConfig)
            .Without(x => x.ResultFilterResultConfig)
            .Create();
        config.PointFilters.Add(filter);
        config.Scorings.ForEeach(x => { x.PointsRule = pointRule; });
        dbContext.PointRules.Add(pointRule);
        dbContext.ResultConfigurations.Add(config);
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            var testFilter = sessionConfig.PointRule.GetPointFilters().FirstOrDefault() as ColumnValueRowFilter;
            testFilter.Should().NotBeNull();
            testFilter!.ColumnProperty.Name.Should().Be(condition.ColumnPropertyName);
            testFilter.Comparator.Should().Be(condition.Comparator);
            testFilter.FilterValues.Should().BeEquivalentTo(condition.FilterValues);
            testFilter.Action.Should().Be(condition.Action);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvidePointRuleWithResultFilters_WhenResultConfigHasResultFilters()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
        var condition = fixture.Build<FilterConditionEntity>()
                    .With(x => x.ColumnPropertyName, nameof(ResultRowCalculationResult.Firstname))
                    .Without(x => x.FilterOption)
                    .Create();
        var filter = fixture.Build<FilterOptionEntity>()
            .With(x => x.Conditions, new[]
            {
                    condition,
            })
            .Without(x => x.PointFilterResultConfig)
            .Without(x => x.ResultFilterResultConfig)
            .Create();
        config.ResultFilters.Add(filter);
        config.Scorings.ForEeach(x => { x.PointsRule = pointRule; });
        dbContext.PointRules.Add(pointRule);
        dbContext.ResultConfigurations.Add(config);
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            var testFilter = sessionConfig.PointRule.GetResultFilters().FirstOrDefault() as ColumnValueRowFilter;
            testFilter.Should().NotBeNull();
            testFilter!.ColumnProperty.Name.Should().Be(condition.ColumnPropertyName);
            testFilter.Comparator.Should().Be(condition.Comparator);
            testFilter.FilterValues.Should().BeEquivalentTo(condition.FilterValues);
            testFilter.Action.Should().Be(condition.Action);
        }
    }

    private SessionCalculationConfigurationProvider CreateSut()
    {
        return fixture.Create<SessionCalculationConfigurationProvider>();
    }

    private async Task<EventEntity> GetFirstEventEntity()
    {
        return await dbContext.Events
            .Include(x => x.Schedule.Season.League)
            .Include(x => x.Sessions)
            .FirstAsync();
    }
}
