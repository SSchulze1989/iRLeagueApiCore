using AutoFixture.Dsl;
using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Server.Models.ResultsParsing;
using iRLeagueApiCore.UnitTests.Extensions;
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
        private readonly LeagueDbContext dbContext;
        private readonly ILogger<UploadResultHandler> logger;

        public UploadResultHandlerTests(DbTestFixture dbFixture)
        {
            this.dbFixture = dbFixture;
            fixture = new();
            dbContext = dbFixture.CreateDbContext();
            logger = Mock.Of<ILogger<UploadResultHandler>>();
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task CreateFakeResult_ShouldReturnDefaultResult()
        {            
            var result = await CreateFakeResult();

            result.session_results.Should().HaveCount(3);
            result.session_results[0].simsession_name.Should().Be("RACE");
            result.session_results[0].simsession_number.Should().Be(0);
            result.session_results[0].simsession_type.Should().Be((int)SimSessionType.Race);
            result.session_results[0].simsession_type_name.Should().Be("Race");
            result.session_results[1].simsession_name.Should().Be("QUALIFY");
            result.session_results[1].simsession_number.Should().Be(-1);
            result.session_results[1].simsession_type.Should().Be((int)SimSessionType.LoneQualifying);
            result.session_results[1].simsession_type_name.Should().Be("Lone Qualifying");
            result.session_results[2].simsession_name.Should().Be("PRACTICE");
            result.session_results[2].simsession_number.Should().Be(-2);
            result.session_results[2].simsession_type.Should().Be((int)SimSessionType.OpenPractice);
            result.session_results[2].simsession_type_name.Should().Be("Open Practice");
        }

        [Fact]
        public async Task Handle_ShouldCreateMember_WhenMemberDoesNotExist()
        {
            (var firstName, var lastName) = (fixture.Create<string>(), fixture.Create<string>());
            var newMemberRow = fixture.Build<ParseSessionResultRow>()
                .With(x => x.display_name, $"{firstName} {lastName}")
                .Create();
            var result = await CreateFakeResult(false, false, 1);
            result.session_results.First().results = result.session_results.First().results.Append(newMemberRow).ToArray();
            var sut = CreateSut();
            var request = CreateRequest(1, 1, result);

            await sut.Handle(request, default);

            var newMember = await dbContext.Members
                .FirstOrDefaultAsync(x => x.IRacingId == newMemberRow.cust_id.ToString());
            newMember.Should().NotBeNull();
            newMember.Firstname.Should().Be(firstName);
            newMember.Lastname.Should().Be(lastName);
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
            var result = await CreateFakeResult();
            var request = CreateRequest(@event.LeagueId, @event.EventId, result);
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
            var result = await CreateFakeResult();
            var request = CreateRequest(@event.LeagueId, @event.EventId, result);
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
            var result = await CreateFakeResult();
            var request = CreateRequest(@event.LeagueId, @event.EventId, result);
            var sut = CreateSut();

            await sut.Handle(request, default);

            var testSession = await dbContext.Sessions
                .Include(x => x.SessionResult)
                .FirstAsync(x => x.SessionId == session.SessionId);
            var sessionResult = testSession.SessionResult;
            sessionResult.Should().NotBeNull();
            sessionResult.SimSessionType.Should().Be(SimSessionType.Race);
        }

        [Fact]
        public async Task Handle_ShouldAssignHeatRaces()
        {
            var sessionCount = 3;
            var @event = EventBuilder().Create();
            @event.Sessions = SessionBuilder()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.SessionType, SessionType.Race)
                .With(x => x.Name, "Heat")
                .CreateMany(sessionCount)
                .ToList();
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            var result = await CreateFakeResult(raceCount: sessionCount);
            var request = CreateRequest(@event.LeagueId, @event.EventId, result);
            var sut = CreateSut();

            await sut.Handle(request, default);

            var raceResults = result.session_results
                .Where(x => x.simsession_type == (int)SimSessionType.Race)
                .Reverse();
            var testSessions = await dbContext.Sessions
                .Include(x => x.SessionResult)
                    .ThenInclude(x => x.ResultRows)
                .Where(x => x.EventId == @event.EventId)
                .OrderBy(x => x.SessionNr)
                .ToListAsync();
            testSessions[0].SessionResult.Should().NotBeNull();
            foreach ((var testSession, var sessionResult) in testSessions.OrderBy(x => x.SessionNr).Zip(raceResults))
            {
                foreach ((var testRow, var resultRow) in testSession.SessionResult.ResultRows.OrderBy(x => x.FinishPosition).Zip(sessionResult.results))
                {
                    testRow.FinishPosition.Should().Be(resultRow.position);
                    testRow.Member.IRacingId.Should().Be(resultRow.cust_id.ToString());
                }
            }
        }

        [Fact]
        public async Task Handle_ShouldFillResultRowData()
        {
            var @event = EventBuilder().Create();
            @event.Sessions.Add(SessionBuilder()
                .With(x => x.SessionType, SessionType.Race)
                .Create());
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            var result = await CreateFakeResult(false, false, 1);
            var request = CreateRequest(@event.LeagueId, @event.EventId, result);
            var sut = CreateSut();

            await sut.Handle(request, default);

            var session = await dbContext.SessionResults
                .Include(x => x.ResultRows)
                    .ThenInclude(x => x.Member)
                .FirstAsync(x => x.EventId == @event.EventId);
            var laps = result.session_results.First().results.Select(y => y.laps_complete).Max();
            foreach((var testRow, var resultRow) in session.ResultRows.Zip(result.session_results.First().results))
            {
                testRow.AvgLapTime.Should().Be(TimeSpan.FromSeconds(resultRow.average_lap / 10000D));
                testRow.CarId.Should().Be(resultRow.car_id);
                testRow.CarNumber.ToString().Should().Be(resultRow.livery.car_number);
                testRow.ClassId.Should().Be(resultRow.car_class_id);
                testRow.ClubId.Should().Be(resultRow.club_id);
                testRow.CompletedLaps.Should().Be(resultRow.laps_complete);
                testRow.CompletedPct.Should().Be(resultRow.laps_complete / laps);
                testRow.Division.Should().Be(resultRow.division);
                testRow.FastestLapTime.Should().Be(TimeSpan.FromSeconds(resultRow.best_lap_time / 10000D));
                testRow.FastLapNr.Should().Be(resultRow.best_lap_num);
                testRow.FinishPosition.Should().Be(resultRow.position);
                testRow.Incidents.Should().Be(resultRow.incidents);
                testRow.Interval.Should().Be(TimeSpan.FromSeconds(resultRow.interval / 10000D));
                testRow.IRacingId.Should().Be(resultRow.cust_id.ToString());
                testRow.LeadLaps.Should().Be(resultRow.laps_lead);
                testRow.Member.IRacingId.Should().Be(resultRow.cust_id.ToString());
                testRow.NewCpi.Should().Be((int)resultRow.new_cpi);
                testRow.NewIRating.Should().Be(resultRow.newi_rating);
                testRow.NewLicenseLevel.Should().Be(resultRow.new_license_level);
                testRow.NewSafetyRating.Should().Be(resultRow.new_sub_level);
                testRow.OldCpi.Should().Be((int)resultRow.old_cpi);
                testRow.OldIRating.Should().Be(resultRow.oldi_rating);
                testRow.OldLicenseLevel.Should().Be(resultRow.old_license_level);
                testRow.OldSafetyRating.Should().Be(resultRow.old_sub_level);
                testRow.PointsEligible.Should().Be(true);
                testRow.PositionChange.Should().Be(resultRow.position - resultRow.starting_position);
                testRow.QualifyingTime.Should().Be(TimeSpan.FromSeconds(resultRow.qual_lap_time / 10000D));
                testRow.QualifyingTimeAt.Should().Be(resultRow.best_qual_lap_at);
                testRow.StartPosition.Should().Be(resultRow.starting_position);
                testRow.Status.Should().Be(resultRow.reason_out_id);
            }
        }

        private IPostprocessComposer<EventEntity> EventBuilder()
        {
            return fixture.Build<EventEntity>()
                .With(x => x.LeagueId, dbContext.Leagues.Select(x => x.Id).Max() + 1)
                .With(x => x.EventId, () => dbContext.Events.Select(x => x.EventId).Max() + 1)
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
                .With(x => x.SessionId, () => dbContext.Sessions.Select(x => x.SessionId).Max() + fixture.Create<int>())
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

        private async Task<ParseSimSessionResult> CreateFakeResult(bool practice = true,
            bool qualy = true,
            int raceCount = 1)
        {
            var memberCount = 10;
            var members = await dbContext.LeagueMembers
                .Include(x => x.Member)
                .Take(memberCount)
                .ToListAsync();
            var result = fixture.Build<ParseSimSessionResult>()
                .Without(x => x.session_results)
                .Create();
            var sessionResults = new List<ParseSessionResult>();
            var sessionNr = (practice ? -1 : 0) + (qualy ? -1 : 0) - raceCount*2 + 1;
            if (practice)
            {
                sessionResults.Add(SessionResultBuilder(members)
                    .With(x => x.simsession_number, ++sessionNr)
                    .With(x => x.simsession_type, (int)SimSessionType.OpenPractice)
                    .With(x => x.simsession_type_name, "Open Practice")
                    .With(x => x.simsession_name, "PRACTICE")
                    .Create());
            }
            if (qualy)
            {
                sessionResults.Add(SessionResultBuilder(members)
                    .With(x => x.simsession_number, ++sessionNr)
                    .With(x => x.simsession_type, (int)SimSessionType.LoneQualifying)
                    .With(x => x.simsession_type_name, "Lone Qualifying")
                    .With(x => x.simsession_name, "QUALIFY")
                    .Create());

            }
            var heatNr = 1;
            for (int i=0; i<raceCount; i++)
            {
                sessionResults.Add(SessionResultBuilder(members)
                    .With(x => x.simsession_number, ++sessionNr)
                    .With(x => x.simsession_type, (int)SimSessionType.Race)
                    .With(x => x.simsession_type_name, "Race")
                    .With(x => x.simsession_name, raceCount == 1 ? "RACE" : sessionNr == 0 ? "FEATURE" : $"HEAT {heatNr++}")
                    .Create());
                if (sessionNr != 0)
                {
                    sessionResults.Add(SessionResultBuilder(members)
                        .With(x => x.simsession_number, ++sessionNr)
                        .With(x => x.simsession_type, (int)SimSessionType.OpenPractice)
                        .With(x => x.simsession_type_name, "Open Practice")
                        .With(x => x.simsession_name, "WARMUP")
                        .Create());
                }
            }
            sessionResults.Reverse();
            result.session_results = sessionResults.ToArray();
            return result;
        }

        private IPostprocessComposer<ParseSessionResult> SessionResultBuilder(IEnumerable<LeagueMemberEntity> members)
        {
            return fixture.Build<ParseSessionResult>()
                .With(x => x.results, members.Shuffle().Select((member, i) => CreateResultRow(i+1, member)).ToArray());
        }

        private ParseSessionResultRow CreateResultRow(int pos, LeagueMemberEntity leagueMember)
        {
            return fixture.Build<ParseSessionResultRow>()
                .With(x => x.position, pos)
                .With(x => x.cust_id, Convert.ToInt64(leagueMember.Member.IRacingId))
                .With(x => x.display_name, GetMemberFullName(leagueMember.Member))
                .With(x => x.livery, fixture.Build<ParseLivery>()
                    .With(x => x.car_number, fixture.Create<int>().ToString())
                    .Create())
                .Create();
        }

        private string GetMemberFullName(MemberEntity member)
        {
            return $"{member.Firstname} {member.Firstname}";
        }

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
        }
    }
}
