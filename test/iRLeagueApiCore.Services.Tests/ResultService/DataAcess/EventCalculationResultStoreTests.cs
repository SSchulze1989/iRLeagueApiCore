using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess
{
    [Collection("DataAccessTests")]
    public class EventCalculationResultStoreTests : IAsyncLifetime
    {
        private readonly Fixture fixture;
        private readonly DataAccessMockHelper accessMockHelper;
        private readonly LeagueDbContext dbContext;

        public EventCalculationResultStoreTests()
        {
            fixture = new();
            accessMockHelper = new();
            dbContext = accessMockHelper.CreateMockDbContext();
            fixture.Register(() => dbContext);
        }

        public async Task InitializeAsync()
        {
            await accessMockHelper.PopulateBasicTestSet(dbContext);
        }

        [Fact]
        public async Task StoreCalculationResult_ShouldStoreNewResult_WhenResultNotExists()
        {
            var @event = await GetFirstEventEntity();
            var configEntity = accessMockHelper.CreateConfiguration(@event);
            var config = GetConfiguration(@event);
            var result = GetCalculationResult(@event, config);
            dbContext.ResultConfigurations.Add(configEntity);
            await dbContext.SaveChangesAsync();
            var sut = CreateSut();
            dbContext.ScoredEventResults
                .Where(x => x.EventId == @event.EventId)
                .Should().BeEmpty();

            await sut.StoreCalculationResult(result);

            dbContext.ScoredEventResults
                .Where(x => x.EventId == @event.EventId)
                .Should().NotBeEmpty();
        }

        [Fact]
        public async Task StoreCalculationResult_ShouldStoreSameResult_WhenResultExists()
        {
            var @event = await GetFirstEventEntity();
            var configEntity = accessMockHelper.CreateConfiguration(@event);
            var resultEntity = accessMockHelper.CreateScoredResult(@event, configEntity);
            var testRowEntity = resultEntity.ScoredSessionResults.First().ScoredResultRows.First();
            dbContext.ResultConfigurations.Add(configEntity);
            dbContext.ScoredEventResults.Add(resultEntity);
            await dbContext.SaveChangesAsync();
            var config = GetConfiguration(@event, resultEntity);
            var result = GetCalculationResult(@event, config, resultEntity);
            var testRow = result.SessionResults.First().ResultRows.First(x => x.MemberId == testRowEntity.MemberId);
            testRowEntity.RacePoints = 0d;
            testRow.RacePoints = fixture.Create<double>();
            var sut = CreateSut();

            await sut.StoreCalculationResult(result);

            resultEntity = await dbContext.ScoredEventResults
                .FirstAsync(x => x.ResultId == config.ResultId);
            testRowEntity = resultEntity.ScoredSessionResults.First().ScoredResultRows
                .First(x => x.MemberId == testRow.MemberId);
            testRowEntity.RacePoints.Should().Be(testRow.RacePoints);
        }

        private EventCalculationResultStore CreateSut()
        {
            return fixture.Create<EventCalculationResultStore>();
        }

        private async Task<EventEntity> GetFirstEventEntity()
        {
            return await dbContext.Events
                .Include(x => x.Schedule.Season.League)
                .Include(x => x.Sessions)
                    .ThenInclude(x => x.IncidentReviews)
                .Include(x => x.ScoredEventResults)
                .FirstAsync();
        }

        private EventCalculationConfiguration GetConfiguration(EventEntity @event, ScoredEventResultEntity? scoredEventResult = null)
        {
            return fixture.Build<EventCalculationConfiguration>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.EventId, @event.EventId)
                .With(x => x.ResultConfigId, scoredEventResult?.ResultConfigId)
                .With(x => x.ResultId, scoredEventResult?.ResultId)
                .With(x => x.SessionResultConfigurations, @event.Sessions
                    .Zip(scoredEventResult?.ScoredSessionResults ?? Array.Empty<ScoredSessionResultEntity>())
                    .Select(z => fixture.Build<SessionCalculationConfiguration>()
                        .With(x => x.SessionId, z.First.SessionId)
                        .With(x => x.SessionResultId, z.Second.SessionResultId)
                        .Create())
                    .ToList())
                .Create();
        }

        private EventCalculationResult GetCalculationResult(EventEntity @event, EventCalculationConfiguration config, ScoredEventResultEntity? resultEntity = null)
        {
            return fixture.Build<EventCalculationResult>()
                .With(x => x.EventId, @event.EventId)
                .With(x => x.ResultConfigId, config.ResultConfigId)
                .With(x => x.ResultId, config.ResultId)
                .With(x => x.SessionResults, resultEntity?.ScoredSessionResults.Select(sessionResult => fixture.Build<SessionCalculationResult>()
                    .With(x => x.LeagueId, sessionResult.LeagueId)
                    .With(x => x.SessionResultId, sessionResult.SessionResultId)
                    .With(x => x.ResultRows, sessionResult.ScoredResultRows.Select(resultRow => fixture.Build<ResultRowCalculationResult>()
                        .With(x => x.MemberId, resultRow.MemberId)
                        .With(x => x.TeamId, resultRow.TeamId)
                        .Create()).ToList())
                    .Create()).ToList()
                    ?? config.SessionResultConfigurations.Select(sessionConfig => fixture.Build<SessionCalculationResult>()
                    .With(x => x.LeagueId, sessionConfig.LeagueId)
                    .With(x => x.SessionResultId, sessionConfig.SessionResultId)
                    .Create()).ToList())
                .Create();
        }

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
        }
    }
}
