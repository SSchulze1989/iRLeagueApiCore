using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAccess;

public sealed class EventCalculationResultStoreTests : DataAccessTestsBase
{
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

    [Fact]
    public async Task StoreCalculationResult_ShouldDeleteSessionResult_WhenNotInCalculationResult()
    {
        var @event = await GetFirstEventEntity();
        var configEntity = accessMockHelper.CreateConfiguration(@event);
        var resultEntity = accessMockHelper.CreateScoredResult(@event, configEntity);
        var removeScoring = configEntity.Scorings.OrderBy(x => x.Index).Last();
        var removeSessionResult = resultEntity.ScoredSessionResults.ElementAt(1);
        dbContext.ResultConfigurations.Add(configEntity);
        dbContext.ScoredEventResults.Add(resultEntity);
        await dbContext.SaveChangesAsync();
        var config = GetConfiguration(@event, resultEntity);
        config.SessionResultConfigurations = config.SessionResultConfigurations
            .Where(x => x.SessionResultId != removeSessionResult.SessionResultId);
        var result = GetCalculationResult(@event, config);
        var sut = CreateSut();

        await sut.StoreCalculationResult(result);

        var test = await dbContext.ScoredSessionResults
            .Where(x => x.SessionResultId == removeSessionResult.SessionResultId)
            .SingleOrDefaultAsync();
        test.Should().BeNull();
    }

    [Fact]
    public async Task StoreCalculationResult_ShouldDeleteResultRow_WhenNotInSessionResult()
    {
        var @event = await GetFirstEventEntity();
        var configEntity = accessMockHelper.CreateConfiguration(@event);
        var resultEntity = accessMockHelper.CreateScoredResult(@event, configEntity);
        var removeScoring = configEntity.Scorings.OrderBy(x => x.Index).Last();
        var removeRow = resultEntity.ScoredSessionResults
            .OrderBy(x => x.SessionNr)
            .Last()
            .ScoredResultRows.OrderBy(x => x.FinishPosition)
            .ElementAt(1);
        dbContext.ResultConfigurations.Add(configEntity);
        dbContext.ScoredEventResults.Add(resultEntity);
        await dbContext.SaveChangesAsync();
        var config = GetConfiguration(@event, resultEntity);
        var result = GetCalculationResult(@event, config, resultEntity);
        var removeRowFromSessionResult = result.SessionResults.Single(x => x.SessionResultId == removeRow.SessionResultId);
        removeRowFromSessionResult.ResultRows = removeRowFromSessionResult.ResultRows
            .Where(x => x.MemberId != removeRow.MemberId);
        var sut = CreateSut();

        await sut.StoreCalculationResult(result);

        var testResult = await dbContext.ScoredSessionResults
            .SingleAsync(x => x.SessionNr == removeRowFromSessionResult.SessionNr);
        testResult.ScoredResultRows.Should().HaveSameCount(removeRowFromSessionResult.ResultRows);
        testResult.ScoredResultRows.Should().NotContain(x => x.ScoredResultRowId == removeRow.ScoredResultRowId);
    }

    [Fact]
    public async Task StoreCalculationResult_ShouldStoreTeamResultRows_WhenIsTeamResult()
    {
        var @event = await GetFirstEventEntity();
        var members = await dbContext.LeagueMembers.ToListAsync();
        @event.Sessions = @event.Sessions.Where(x => x.SessionType == SessionType.Race).Take(1).ToList();
        var sourceConfigEntity = accessMockHelper.CreateConfiguration(@event);
        var configEntity = accessMockHelper.ConfigurationBuilder(@event)
            .With(x => x.SourceResultConfig, sourceConfigEntity)
            .Create();
        var sourceResultEntity = accessMockHelper.CreateScoredResult(@event, sourceConfigEntity);
        dbContext.ResultConfigurations.Add(sourceConfigEntity);
        dbContext.ResultConfigurations.Add(configEntity);
        dbContext.ScoredEventResults.Add(sourceResultEntity);
        await dbContext.SaveChangesAsync();
        var config = GetConfiguration(@event, configEntity);
        var result = GetCalculationResult(@event, config);
        result.SessionResults.Single().ResultRows.ForEeach(x => x.MemberId = null);
        result.SessionResults.Single().ResultRows.First().ScoredMemberResultRowIds = sourceResultEntity
            .ScoredSessionResults.Single().ScoredResultRows.Take(2).Select(x => x.ScoredResultRowId).ToList();
        var sut = CreateSut();

        await sut.StoreCalculationResult(result);

        var testResult = await dbContext.ScoredEventResults
            .Include(x => x.ScoredSessionResults)
                .ThenInclude(x => x.ScoredResultRows)
                    .ThenInclude(x => x.TeamResultRows)
            .Where(x => x.ResultConfigId == config.ResultConfigId)
            .SingleAsync(x => x.EventId == @event.EventId);
        var teamRows = testResult.ScoredSessionResults.Single().ScoredResultRows.First().TeamResultRows;
        teamRows.Should().NotBeEmpty();
        teamRows.Should().HaveCount(2);
        teamRows.Select(x => x.ScoredResultRowId).Should().BeEquivalentTo(
            sourceResultEntity.ScoredSessionResults.Single().ScoredResultRows.Select(x => x.ScoredResultRowId).Take(2));
    }

    [Fact]
    public async Task StoreCalculationResult_ShouldStoreResult_WithChampSeason()
    {
        var @event = await GetFirstEventEntity();
        var championship = await dbContext.Championships.FirstAsync();
        var champSeason = accessMockHelper.CreateChampSeason(championship, @event.Schedule.Season);
        var configEntity = accessMockHelper.CreateConfiguration(@event);
        var config = GetConfiguration(@event, configEntity);
        var result = GetCalculationResult(@event, config);
        result.ChampSeasonId = champSeason.ChampSeasonId;
        dbContext.ChampSeasons.Add(champSeason);
        dbContext.ResultConfigurations.Add(configEntity);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        await sut.StoreCalculationResult(result);

        var test = await dbContext.ScoredEventResults
            .Where(x => x.EventId == @event.EventId)
            .Where(x => x.ResultConfigId == configEntity.ResultConfigId)
            .FirstOrDefaultAsync();
        test.Should().NotBeNull();
        test!.ChampSeasonId.Should().Be(champSeason.ChampSeasonId);
    }

    private EventCalculationResultStore CreateSut()
    {
        return fixture.Create<EventCalculationResultStore>();
    }

    private async Task<EventEntity> GetFirstEventEntity()
    {
        return await dbContext.Events
            .Include(x => x.Schedule.Season)
                .ThenInclude(x => x.League)
            .Include(x => x.Sessions)
                .ThenInclude(x => x.IncidentReviews)
            .Include(x => x.ScoredEventResults)
            .FirstAsync();
    }

    private EventCalculationConfiguration GetConfiguration(EventEntity @event, ResultConfigurationEntity config)
    {
        return fixture.Build<EventCalculationConfiguration>()
            .With(x => x.LeagueId, @event.LeagueId)
            .With(x => x.EventId, @event.EventId)
            .With(x => x.ResultConfigId, config.ResultConfigId)
            .With(x => x.SourceResultConfigId, config.SourceResultConfigId)
            .With(x => x.SessionResultConfigurations, @event.Sessions
                .Select(z => fixture.Build<SessionCalculationConfiguration>()
                    .With(x => x.SessionId, z.SessionId)
                    .Without(x => x.SessionResultId)
                    .Create())
                .ToList())
            .Without(x => x.ResultId)
            .Create();
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

    private EventCalculationResult GetCalculationResult(EventEntity @event, EventCalculationConfiguration config,
        ScoredEventResultEntity? resultEntity = null, IEnumerable<LeagueMemberEntity>? members = null)
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
                .With(x => x.ResultRows, (members?.Select(member => fixture.Build<ResultRowCalculationResult>()
                    .With(x => x.MemberId, member.MemberId)
                    .With(x => x.TeamId, member.TeamId)
                    .Create())
                    ?? fixture.CreateMany<ResultRowCalculationResult>()).ToList())
                .Create()).ToList())
            .Create();
    }
}
