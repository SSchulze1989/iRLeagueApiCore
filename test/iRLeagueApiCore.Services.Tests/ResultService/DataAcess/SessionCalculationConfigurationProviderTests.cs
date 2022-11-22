using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess
{
    [Collection("DataAccessTests")]
    public class SessionCalculationConfigurationProviderTests : IAsyncLifetime
    {
        private readonly Fixture fixture;
        private readonly DataAccessMockHelper accessMockHelper;
        private readonly LeagueDbContext dbContext;

        public SessionCalculationConfigurationProviderTests()
        {
            accessMockHelper = new ();
            dbContext = accessMockHelper.CreateMockDbContext();
            fixture = new();
            fixture.Register(() => dbContext);
        }

        public async Task InitializeAsync()
        {
            await accessMockHelper.PopulateBasicTestSet(dbContext);
        }

        [Fact]
        public async Task GetConfigurations_ShouldProvideDefaultConfiguration_WhenConfigIsNull()
        {
            var @event = await GetFirstEventEntity();
            var config = (ResultConfigurationEntity?)null;
            var sut = fixture.Create<SessionCalculationConfigurationProvider>();

            var test = await sut.GetConfigurations(@event, config);

            test.Should().HaveSameCount(@event.Sessions);
            foreach((var sessionConfig, var session) in test.Zip(@event.Sessions.OrderBy(x => x.SessionNr)))
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
            foreach((var sessionConfig, var sessionResult) in test.Zip(scoredEventResult.ScoredSessionResults.OrderBy(x => x.SessionNr)))
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
            foreach((var sessionConfig, var session, var scoring) in test.Zip(@event.Sessions.OrderBy(x => x.SessionNr), config.Scorings.OrderBy(x => x.Index)))
            {
                sessionConfig.LeagueId.Should().Be(@event.LeagueId);
                sessionConfig.SessionId.Should().Be(session.SessionId);
                sessionConfig.ScoringId.Should().Be(scoring.ScoringId);
                sessionConfig.MaxResultsPerGroup.Should().Be(scoring.MaxResultsPerGroup);
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
        public async Task GetConfigurations_ShouldProvideDefaultPointRule_WhenScoringOrPointRuleEntityIsNull()
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

            foreach(var sessionConfig in test)
            {
                sessionConfig.PointRule.Should().BeOfType(CalculationPointRuleBase.Default().GetType());
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
            foreach((var sessionConfig, var session) in test.Zip(@event.Sessions.OrderBy(x => x.SessionNr)))
            {
                if (session == practice || session == qualy)
                {
                    sessionConfig.ScoringId.Should().BeNull();
                    sessionConfig.ResultKind.Should().Be(ResultKind.Member);
                    sessionConfig.PointRule.Should().BeOfType(CalculationPointRuleBase.Default().GetType());
                }
                else
                {
                    sessionConfig.ScoringId.Should().Be(config.Scorings.ElementAt(scoringIndex++).ScoringId);
                }
            }
        }

        [Fact]
        public async Task GetConfigurations_ShouldProvideMaxPointRule_WhenNoPointsPerPlaceConfigured()
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

            foreach(var sessionConfig in test)
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

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
        }
    }
}
