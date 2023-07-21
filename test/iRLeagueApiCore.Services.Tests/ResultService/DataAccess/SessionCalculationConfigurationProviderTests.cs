using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAccess;

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
            var filterGroup = sessionConfig.PointRule.GetPointFilters() as FilterGroupRowFilter<ResultRowCalculationResult>;
            var testFilter = filterGroup!.GetFilters().First().rowFilter as ColumnValueRowFilter;
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
            var filterGroup = sessionConfig.PointRule.GetResultFilters() as FilterGroupRowFilter<ResultRowCalculationResult>;
            var testFilter = filterGroup!.GetFilters().First().rowFilter as ColumnValueRowFilter;
            testFilter.Should().NotBeNull();
            testFilter!.ColumnProperty.Name.Should().Be(condition.ColumnPropertyName);
            testFilter.Comparator.Should().Be(condition.Comparator);
            testFilter.FilterValues.Should().BeEquivalentTo(condition.FilterValues);
            testFilter.Action.Should().Be(condition.Action);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideColumnValueRowFilter_WhenConfigured()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var condition = fixture.Build<FilterConditionEntity>()
            .With(x => x.FilterType, FilterType.ColumnProperty)
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
        dbContext.ResultConfigurations.Add(config);
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            var filterGroup = sessionConfig.PointRule.GetResultFilters();
            var testFilter = filterGroup!.GetFilters().First().rowFilter as ColumnValueRowFilter;
            testFilter.Should().NotBeNull();
            testFilter!.ColumnProperty.Name.Should().Be(condition.ColumnPropertyName);
            testFilter.Comparator.Should().Be(condition.Comparator);
            testFilter.FilterValues.Should().BeEquivalentTo(condition.FilterValues);
            testFilter.Action.Should().Be(condition.Action);
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideMemberRowFilter_WhenConfigured()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var condition = fixture.Build<FilterConditionEntity>()
            .With(x => x.FilterType, FilterType.Member)
            .With(x => x.FilterValues, fixture.CreateMany<long>().Select(x => x.ToString()).ToList())
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
        dbContext.ResultConfigurations.Add(config);
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            var filterGroup = sessionConfig.PointRule.GetResultFilters();
            var testFilter = filterGroup!.GetFilters().First().rowFilter as MemberRowFilter;
            testFilter.Should().NotBeNull();
            testFilter!.MemberIds.Select(x => x.ToString()).Should().BeEquivalentTo(condition.FilterValues);
            testFilter.Action.Should().Be(condition.Action);
        }
    }

    [Theory]
    [InlineData(
        MatchedValueAction.Keep, FilterCombination.And, 
        MatchedValueAction.Keep, FilterCombination.Or, 
        MatchedValueAction.Remove, FilterCombination.And)]
    [InlineData(
        MatchedValueAction.Remove, FilterCombination.And,
        MatchedValueAction.Keep, FilterCombination.Or,
        MatchedValueAction.Remove, FilterCombination.And)]
    [InlineData(
        MatchedValueAction.Keep, FilterCombination.And,
        MatchedValueAction.Keep, FilterCombination.Or,
        MatchedValueAction.Keep, FilterCombination.Or)]
    [InlineData(
        MatchedValueAction.Remove, FilterCombination.And,
        MatchedValueAction.Remove, FilterCombination.And,
        MatchedValueAction.Remove, FilterCombination.And)]
    public async Task GetConfigurations_ShouldProvideCombinedFilters_WhenConfigured(
        MatchedValueAction action1, FilterCombination combination1,
        MatchedValueAction action2, FilterCombination combination2,
        MatchedValueAction action3, FilterCombination combination3)
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var condition1 = fixture.Build<FilterConditionEntity>()
            .With(x => x.FilterType, FilterType.ColumnProperty)
            .With(x => x.ColumnPropertyName, nameof(ResultRowCalculationResult.Firstname))
            .With(x => x.Action, action1)
            .Without(x => x.FilterOption)
            .Create();
        var filter1 = fixture.Build<FilterOptionEntity>()
            .With(x => x.Conditions, new[]
            {
                    condition1,
            })
            .Without(x => x.PointFilterResultConfig)
            .Without(x => x.ResultFilterResultConfig)
            .Create();
        var condition2 = fixture.Build<FilterConditionEntity>()
            .With(x => x.FilterType, FilterType.ColumnProperty)
            .With(x => x.ColumnPropertyName, nameof(ResultRowCalculationResult.Lastname))
            .With(x => x.Action, action2)
            .Without(x => x.FilterOption)
            .Create();
        var filter2 = fixture.Build<FilterOptionEntity>()
            .With(x => x.Conditions, new[]
            {
                    condition2,
            })
            .Without(x => x.PointFilterResultConfig)
            .Without(x => x.ResultFilterResultConfig)
            .Create();
        var condition3 = fixture.Build<FilterConditionEntity>()
            .With(x => x.FilterType, FilterType.Member)
            .With(x => x.FilterValues, fixture.CreateMany<long>().Select(x => x.ToString()).ToList())
            .With(x => x.Action, action3)
            .Without(x => x.FilterOption)
            .Create();
        var filter3 = fixture.Build<FilterOptionEntity>()
            .With(x => x.Conditions, new[]
            {
                    condition3,
            })
            .Without(x => x.PointFilterResultConfig)
            .Without(x => x.ResultFilterResultConfig)
            .Create();
        config.ResultFilters.Add(filter1);
        config.ResultFilters.Add(filter2);
        config.ResultFilters.Add(filter3);
        dbContext.ResultConfigurations.Add(config);
        var sut = CreateSut();

        var test = await sut.GetConfigurations(@event, config);

        foreach (var sessionConfig in test)
        {
            var filterGroup = sessionConfig.PointRule.GetResultFilters();
            var testFilter1 = filterGroup!.GetFilters().First();
            testFilter1.combination.Should().Be(combination1);
            testFilter1.rowFilter.Should().BeOfType<ColumnValueRowFilter>();
            var testFilter2 = filterGroup.GetFilters().ElementAt(1);
            testFilter2.combination.Should().Be(combination2);
            testFilter2.rowFilter.Should().BeOfType<ColumnValueRowFilter>();
            var testFilter3 = filterGroup.GetFilters().ElementAt(2);
            testFilter3.combination.Should().Be(combination3);
            testFilter3.rowFilter.Should().BeOfType<MemberRowFilter>();
        }
    }

    [Fact]
    public async Task GetConfigurations_ShouldProvideAutoPenalties()
    {
        var @event = await GetFirstEventEntity();
        var config = accessMockHelper.CreateConfiguration(@event);
        var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
        var autoPenalty = fixture.Build<AutoPenaltyConfigEntity>()
            .With(x => x.Conditions, () => fixture.Build<FilterConditionModel>()
                .With(x => x.ColumnPropertyName, nameof(ResultRowCalculationResult.Incidents))
                .With(x => x.FilterType, FilterType.ColumnProperty)
                .With(x => x.Comparator, ComparatorType.ForEach)
                .With(x => x.FilterValues, new[] { "4.0" })
                .CreateMany(1).ToList())
            .With(x => x.PointRule, pointRule)
            .With(x => x.Description, "Test Autopenalty")
            .With(x => x.Points, 42)
            .With(x => x.Positions, 420)
            .With(x => x.Time, new TimeSpan(1, 2, 3))
            .With(x => x.Type, PenaltyType.Position)
            .Create();
        pointRule.AutoPenalties.Add(autoPenalty);
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
            sessionConfig.PointRule.Should().BeAssignableTo(typeof(CalculationPointRuleBase));
            var sessionConfigPointRule = (CalculationPointRuleBase)sessionConfig.PointRule;
            sessionConfigPointRule.AutoPenalties.Should().HaveCount(1);
            var testPenalty = sessionConfigPointRule.AutoPenalties.First();
            testPenalty.Description.Should().Be("Test Autopenalty");
            testPenalty.Points.Should().Be(42);
            testPenalty.Positions.Should().Be(420);
            testPenalty.Time.Should().Be(new TimeSpan(1, 2, 3));
            testPenalty.Type.Should().Be(PenaltyType.Position);
            testPenalty.Conditions.GetFilters().Should().HaveCount(1);
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
