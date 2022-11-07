using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.DataAccess;
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
            fixture.Register<ILeagueDbContext>(() => dbContext);
        }

        public async Task InitializeAsync()
        {
            await accessMockHelper.PopulateBasicTestSet(dbContext);
        }

        [Fact]
        public async Task GetConfigurations_ShouldProvideDefaultConfiguration_WhenConfigIsNull()
        {
            var @event = await dbContext.Events
                .Include(x => x.Sessions)
                .FirstAsync();
            var config = (ResultConfigurationEntity?)null;
            var sut = fixture.Create<SessionCalculationConfigurationProvider>();

            var test = await sut.GetConfigurations(@event, config);

            test.Should().HaveSameCount(@event.Sessions);
            foreach((var sessionConfig, var session) in test.Zip(@event.Sessions))
            {
                sessionConfig.LeagueId.Should().Be(@event.LeagueId);
                sessionConfig.SessionId.Should().Be(session.SessionId);
                sessionConfig.ScoringId.Should().BeNull();
                sessionConfig.UpdateTeamOnRecalculation.Should().BeFalse();
                sessionConfig.UseResultSetTeam.Should().BeFalse();
                sessionConfig.ScoringKind.Should().Be(ScoringKind.Member);
            }
        }

        [Fact]
        public async Task GetConfigurations_ShouldProvideDefaultSessionResultId_WhenCalculatedResultExistsAndConfigIsNull()
        {
            var @event = await dbContext.Events
                .Include(x => x.Sessions)
                .FirstAsync();
            var config = (ResultConfigurationEntity?)null;
            var scoredEventResult = fixture.Build<ScoredEventResultEntity>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.ScoredSessionResults, @event.Sessions.Select(session => fixture.Build<ScoredSessionResultEntity>()
                    .With(x => x.LeagueId, session.LeagueId)
                    .With(x => x.Name, session.Name)
                    .With(x => x.SessionNr, session.SessionNr)
                    .Without(x => x.Scoring)
                    .Without(x => x.ScoredEventResult)
                    .Without(x => x.ScoredResultRows)
                    .Without(x => x.CleanestDrivers)
                    .Without(x => x.FastestAvgLapDriver)
                    .Without(x => x.FastestLapDriver)
                    .Without(x => x.FastestQualyLapDriver)
                    .Without(x => x.HardChargers)
                    .Create())
                    .ToList())
                .Without(x => x.ResultConfig)
                .Without(x => x.ResultConfigId)
                .Create();
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

        private SessionCalculationConfigurationProvider CreateSut()
        {
            return fixture.Create<SessionCalculationConfigurationProvider>();
        }

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
        }
    }
}
