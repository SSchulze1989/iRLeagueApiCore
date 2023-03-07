using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAccess;

public sealed class EventCalculationDataProviderTests : DataAccessTestsBase
{
    [Fact]
    public async Task GetData_ShouldReturnNull_WhenNoResultDataExists()
    {
        var schedule = await dbContext.Schedules.FirstAsync();
        var @event = accessMockHelper.EventBuilder(schedule).Create();
        var config = GetConfiguration(@event);
        var sut = CreateSut();

        var test = await sut.GetData(config);

        test.Should().BeNull();
    }

    [Fact]
    public async Task GetData_ShouldReturnData_WhenResultDataExists()
    {
        var @event = await GetFirstEventEntity();
        var rawResult = @event.EventResult;
        var config = GetConfiguration(@event);
        var sut = CreateSut();

        var test = (await sut.GetData(config))!;

        test.Should().NotBeNull();
        test.LeagueId = @event.LeagueId;
        test.EventId = @event.EventId;
        test.SessionResults.Should().HaveSameCount(@event.Sessions);
        test.SessionResults.Should().HaveSameCount(rawResult.SessionResults);
        foreach ((var sessionResultData, var session, var sessionResult) in test.SessionResults.Zip(@event.Sessions, rawResult.SessionResults))
        {
            sessionResultData.LeagueId.Should().Be(session.LeagueId);
            sessionResultData.SessionId.Should().Be(session.SessionId);
            sessionResultData.SessionId.Should().Be(sessionResult.SessionId);
            sessionResultData.ResultRows.Should().HaveSameCount(sessionResult.ResultRows);
            sessionResultData.AcceptedReviewVotes.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetData_ShouldProvideAcceptedReviewVotes_WhenAcceptedVoteExists()
    {
        var @event = await GetFirstEventEntity();
        var review = @event.Sessions.First().IncidentReviews.First();
        var memberAtFault = @event.Sessions.First().SessionResult.ResultRows.First().Member;
        var voteCat = await dbContext.VoteCategories.FirstAsync();
        var vote = accessMockHelper.AcceptedReviewVoteBuilder()
            .With(x => x.LeagueId, review.LeagueId)
            .With(x => x.Review, review)
            .With(x => x.ReviewId, review.ReviewId)
            .With(x => x.MemberAtFault, memberAtFault)
            .With(x => x.MemberAtFaultId, memberAtFault.Id)
            .With(x => x.VoteCategory, voteCat)
            .Without(x => x.ReviewPenaltys)
            .Create();
        dbContext.AcceptedReviewVotes.Add(vote);
        await dbContext.SaveChangesAsync();
        var config = GetConfiguration(@event);
        var sut = CreateSut();

        var test = (await sut.GetData(config))!;

        test.SessionResults.First().AcceptedReviewVotes.First().ReviewVoteId.Should().Be(vote.ReviewVoteId);
    }

    [Fact]
    public async Task GetData_ShouldProvideResultFromSoure_WhenSourceConfigIsConfigured()
    {
        var @event = await GetFirstEventEntity();
        var sourceConfig = accessMockHelper.CreateConfiguration(@event);
        var sourceResult = accessMockHelper.CreateScoredResult(@event, sourceConfig);
        dbContext.ResultConfigurations.Add(sourceConfig);
        dbContext.ScoredEventResults.Add(sourceResult);
        await dbContext.SaveChangesAsync();
        var config = GetConfiguration(@event);
        config.SourceResultConfigId = sourceConfig.ResultConfigId;
        var sut = CreateSut();

        var test = await sut.GetData(config);

        test.Should().NotBeNull();
        test!.SessionResults.Should().HaveSameCount(sourceResult.ScoredSessionResults);
        foreach ((var testSession, var sourceSession) in test.SessionResults.OrderBy(x => x.SessionNr).Zip(sourceResult.ScoredSessionResults.OrderBy(x => x.SessionNr)))
        {
            foreach ((var testRow, var sourceRow) in testSession.ResultRows.OrderBy(x => x.MemberId).Zip(sourceSession.ScoredResultRows.OrderBy(x => x.MemberId)))
            {
                testRow.RacePoints.Should().Be(sourceRow.RacePoints);
                testRow.Car.Should().Be(sourceRow.Car);
            }
        }
    }

    private EventCalculationDataProvider CreateSut()
    {
        return fixture.Create<EventCalculationDataProvider>();
    }

    private async Task<EventEntity> GetFirstEventEntity()
    {
        return await dbContext.Events
            .Include(x => x.EventResult)
                .ThenInclude(x => x.SessionResults)
                    .ThenInclude(x => x.ResultRows)
            .Include(x => x.Schedule.Season.League)
            .Include(x => x.Sessions)
                .ThenInclude(x => x.IncidentReviews)
            .FirstAsync();
    }

    private EventCalculationConfiguration GetConfiguration(EventEntity @event)
    {
        return fixture.Build<EventCalculationConfiguration>()
            .With(x => x.LeagueId, @event.LeagueId)
            .With(x => x.EventId, @event.EventId)
            .Without(x => x.SourceResultConfigId)
            .Create();
    }
}
