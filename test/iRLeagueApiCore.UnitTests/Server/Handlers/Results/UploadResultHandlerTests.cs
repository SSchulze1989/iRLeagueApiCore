using AutoFixture.Dsl;
using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Server.Models.ResultsParsing;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("DbTestFixture")]
    public sealed class UploadResultHandlerTests : IClassFixture<DbTestFixture>, IAsyncLifetime
    {
        private readonly Fixture fixture;
        private readonly DbTestFixture dbFixture;
        private const string UploadFileName = "Data/iracing-result.json";
        private readonly LeagueDbContext dbContext;
        private readonly ILogger<UploadResultHandler> logger;
        private ParseSimSessionResult realTestData;

        public UploadResultHandlerTests(DbTestFixture dbFixture)
        {
            this.dbFixture = dbFixture;
            fixture = new();
            dbContext = dbFixture.CreateDbContext();
            logger = Mock.Of<ILogger<UploadResultHandler>>();
        }

        public async Task InitializeAsync()
        {
            using var stream = GetTestStream();
            realTestData = await JsonSerializer.DeserializeAsync<ParseSimSessionResult>(stream);
        }

        [Fact]
        public async Task Handle_ShoulNotThrow_WhenUsingTestJson()
        {
            var logger = Mock.Of<ILogger<UploadResultHandler>>();
            var sut = CreateSut();
            var request = CreateRequest(1, 1, realTestData);

            await sut.Handle(request, default);
        }

        [Fact]
        public async Task Handle_ShouldCreateMember_WhenMemberDoesNotExist()
        {
            var sut = CreateSut();
            var request = CreateRequest(1, 1, realTestData);

            await sut.Handle(request, default);

            var newMember = await dbContext.Members
                .FirstOrDefaultAsync(x => x.IRacingId == "420");
            newMember.Should().NotBeNull();
            newMember.Firstname.Should().Be("New");
            newMember.Lastname.Should().Be("Member Guy");
        }

        [Fact(Skip = "Not implemented")]
        public async Task Handle_ShouldUpdateMember_WhenMemberDoesExist()
        {
            var memberId = await dbContext.Members.Select(x => x.IRacingId).FirstAsync();
            var jsonData = realTestData;
            var modCustId = jsonData.session_results
                .SelectMany(x => x.results)
                .Where(x => x.cust_id == 1 && x.display_name == "Member 1");
            foreach (var entry in modCustId)
            {
                entry.cust_id = long.Parse(memberId);
            }
            var request = CreateRequest(1, 1, jsonData);
            var sut = CreateSut();

            await sut.Handle(request, default);

            var testMember = await dbContext.Members.FirstAsync();
            testMember.Firstname.Should().Be("Member");
            testMember.Lastname.Should().Be("1");
        }

        [Fact]
        public async Task Handle_ShouldAssingPracticeResults()
        {
            var @event = EventBuilder().Create();
            var session = SessionBuilder()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.SessionType, SessionType.Practice)
                .Create();
            @event.Sessions.Add(session);
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            var request = CreateRequest(@event.LeagueId, @event.EventId, realTestData);
            var sut = CreateSut();

            await sut.Handle(request, default);

            var testSession = await dbContext.Sessions
                .Include(x => x.SessionResult)
                .FirstAsync(x => x.SessionId == session.SessionId);
            var sessionResult = testSession.SessionResult;
            sessionResult.Should().NotBeNull();
            sessionResult.Session.SessionId.Should().Be(session.SessionId);
            sessionResult.SimSessionType.Should().Be(SimSessionType.OpenPractice);
        }

        [Fact]
        public async Task Handle_ShouldAssingQualifyingResults()
        {
            var @event = EventBuilder().Create();
            var session = SessionBuilder()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.SessionType, SessionType.Qualifying)
                .Create();
            @event.Sessions.Add(session);
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            var request = CreateRequest(@event.LeagueId, @event.EventId, realTestData);
            var sut = CreateSut();

            await sut.Handle(request, default);

            var testSession = await dbContext.Sessions
                .Include(x => x.SessionResult)
                .FirstAsync(x => x.SessionId == session.SessionId);
            var sessionResult = testSession.SessionResult;
            sessionResult.Should().NotBeNull();
            sessionResult.SimSessionType.Should().Be(SimSessionType.LoneQualifying);
        }

        [Fact]
        public async Task Handle_ShouldAssignSingleRace()
        {
            var @event = EventBuilder().Create();
            var session = SessionBuilder()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.SessionType, SessionType.Race)
                .Create();
            @event.Sessions.Add(session);
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            var request = CreateRequest(@event.LeagueId, @event.EventId, );
            var sut = CreateSut();

            await sut.Handle(request, default);

            var testSession = await dbContext.Sessions
                .Include(x => x.SessionResult)
                .FirstAsync(x => x.SessionId == session.SessionId);
            var sessionResult = testSession.SessionResult;
            sessionResult.Should().NotBeNull();
            sessionResult.SimSessionType.Should().Be(SimSessionType.Race);
        }

        private IPostprocessComposer<EventEntity> EventBuilder()
        {
            return fixture.Build<EventEntity>()
                .Without(x => x.EventResult)
                .Without(x => x.ResultConfigs)
                .Without(x => x.Schedule)
                .Without(x => x.ScoredEventResults)
                .Without(x => x.Sessions)
                .Without(x => x.Track);
        }

        private IPostprocessComposer<SessionEntity> SessionBuilder()
        {
            return fixture.Build<SessionEntity>()
                .Without(x => x.Event)
                .Without(x => x.IncidentReviews)
                .Without(x => x.SessionResult);
        }

        private UploadResultHandler CreateSut()
        {
            return new UploadResultHandler(logger, dbContext, Array.Empty<IValidator<UploadResultRequest>>());
        }

        private UploadResultRequest CreateRequest(long leagueId, long eventId, ParseSimSessionResult data)
        {
            return new UploadResultRequest(leagueId, eventId, data);
        }

        private async Task<ParseSimSessionResult> CreateFakeResult(EventEntity @event)
        {
            var members = await dbContext.LeagueMembers
                .Include(x => x.Member)
                .Take(3)
                .ToListAsync();
            var result = fixture.Build<ParseSimSessionResult>()
                .With(x => x.session_results, new []
                {
                    fixture.Build<ParseSessionResult>()
                        
                })
        }

        private ParseSessionResultRow CreateResultRow(LeagueMemberEntity leagueMember)
        {
            return fixture.Build<ParseSessionResultRow>()
                .With(x => x.cust_id, leagueMember.Member.IRacingId)
                .With(x => x.display_name, leagueMember)
        }

        private Stream GetTestStream()
        {
            return new FileStream(UploadFileName, FileMode.Open, FileAccess.Read);
        }

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
        }
    }
}
