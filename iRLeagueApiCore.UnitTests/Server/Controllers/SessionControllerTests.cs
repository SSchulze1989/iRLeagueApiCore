using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    public class SessionControllerTests : IClassFixture<DbTestFixture>
    {
        DbTestFixture Fixture { get; }
        ITestOutputHelper Output { get; }

        ILogger<SessionsController> MockLogger => new Mock<ILogger<SessionsController>>().Object;

        public SessionControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
        }

        [Fact]
        public async void GetSession()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                const long testSessionId = 1;
                const string testSessionName = "S1 Session 1";

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Get(testLeagueName, testLeagueId, new long[] { testSessionId })).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (IEnumerable<GetSessionModel>)okResult.Value;
                var session = resultValue.FirstOrDefault();

                Assert.NotNull(session);
                Assert.Equal(testSessionId, session.SessionId);
                Assert.Equal(testSessionName, session.Name);
            }
        }

        [Fact]
        public async void GetMultipleSessions()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                long[] testSessionIds = { 1, 2, 3 };
                string[] testSessionNames = { "S1 Session 1", "S1 Session 2", "S1 Session 3" };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Get(testLeagueName, testLeagueId, testSessionIds)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (IEnumerable<GetSessionModel>)okResult.Value;
                var sessions = resultValue;

                Assert.NotNull(sessions);
                for (int i = 0; i < 3; i++)
                {
                    Assert.Equal(testSessionIds[i], sessions.ElementAt(i).SessionId);
                    Assert.Equal(testSessionNames[i], sessions.ElementAt(i).Name);
                }
            }
        }

        [Fact]
        public async void GetSessionNotFound()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                long[] testSessionIds = { 8 };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Get(testLeagueName, testLeagueId, testSessionIds)).Result;
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void CreateSessionWithoutSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                const string testSessionName = "Session Without Schedule";
                DateTime testDate = DateTime.Parse("2022/03/16T16:42", CultureInfo.InvariantCulture);

                var putSession = new PutSessionModel()
                {
                    Name = testSessionName,
                    ScheduleId = null,
                    Date = testDate
                };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Put(testLeagueName, testLeagueId, putSession)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSession = (GetSessionModel)okResult.Value;

                Assert.NotEqual(0, getSession.SessionId);
                Assert.Equal(testLeagueId, getSession.LeagueId);
                Assert.Equal(testSessionName, getSession.Name);
                Assert.Null(getSession.ScheduleId);
                Assert.Equal(testDate, getSession.Date);
            }
        }

        [Fact]
        public async void CreateSessionWithSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                const int testScheduleId = 1;
                const string testSessionName = "Session With Schedule";
                DateTime testDate = DateTime.Parse("2022/03/16T16:42", CultureInfo.InvariantCulture);

                var putSession = new PutSessionModel()
                {
                    Name = testSessionName,
                    ScheduleId = 1,
                    Date = testDate
                };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Put(testLeagueName, testLeagueId, putSession)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSession = (GetSessionModel)okResult.Value;

                Assert.NotEqual(0, getSession.SessionId);
                Assert.Equal(testLeagueId, getSession.LeagueId);
                Assert.Equal(testSessionName, getSession.Name);
                Assert.Equal(testScheduleId, getSession.ScheduleId);
                Assert.Equal(testDate, getSession.Date);

                // check if session is in schedule sessions
                var schedule = dbContext.Schedules
                    .Include(x => x.Sessions)
                    .Single(x => x.ScheduleId == testScheduleId);
                Assert.Contains(schedule.Sessions, x => x.SessionId == getSession.SessionId);
            }
        }

        [Fact]
        public async void CreateSessionWithParent()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                const int testParentId = 1;
                const string testSessionName = "Session With Schedule";
                DateTime testDate = DateTime.Parse("2022/03/16T16:42", CultureInfo.InvariantCulture);

                var putSession = new PutSessionModel()
                {
                    Name = testSessionName,
                    ParentSessionId = testParentId,
                    Date = testDate
                };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Put(testLeagueName, testLeagueId, putSession)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSession = (GetSessionModel)okResult.Value;

                Assert.NotEqual(0, getSession.SessionId);
                Assert.Equal(testLeagueId, getSession.LeagueId);
                Assert.Equal(testSessionName, getSession.Name);
                Assert.Equal(testParentId, getSession.ParentSessionId);
                Assert.Equal(testDate, getSession.Date);

                // check if session is in schedule sessions
                var parentSession = dbContext.Sessions
                    .Include(x => x.SubSessions)
                    .Single(x => x.SessionId == testParentId);
                Assert.Contains(parentSession.SubSessions, x => x.SessionId == getSession.SessionId);
            }
        }

        [Fact]
        public async void SessionMoveSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var league = Fixture.Leagues.First();
                var season = league.Seasons.ElementAt(1);
                var fromSchedule = season.Schedules.ElementAt(0);
                var toSchedule = season.Schedules.ElementAt(1);
                var session = fromSchedule.Sessions.First();

                var putSession = new PutSessionModel()
                {
                    SessionId = session.SessionId,
                    ScheduleId = toSchedule.ScheduleId,
                    Name = session.Name,
                    Date = session.Date
                };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Put(league.Name, league.Id, putSession)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSession = (GetSessionModel)okResult.Value;

                Assert.Equal(session.SessionId, getSession.SessionId);
                var checkFromSchedule = dbContext.Schedules.Single(x => x.ScheduleId == fromSchedule.ScheduleId);
                var checkToSchedule = dbContext.Schedules.Single(x => x.ScheduleId == toSchedule.ScheduleId);
                Assert.DoesNotContain(checkFromSchedule.Sessions, x => x.SessionId == session.SessionId);
                Assert.Contains(checkToSchedule.Sessions, x => x.SessionId == session.SessionId);
            }
        }

        [Fact]
        public async void SessionMoveParent()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var league = Fixture.Leagues.First();
                var season = league.Seasons.First();
                var schedule = season.Schedules.First();
                var toParent = schedule.Sessions.ElementAt(1);
                var session = schedule.Sessions.ElementAt(2);

                var putSession = new PutSessionModel()
                {
                    SessionId = session.SessionId,
                    ParentSessionId = toParent.SessionId,
                    Name = session.Name,
                    Date = session.Date
                };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Put(league.Name, league.Id, putSession)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSession = (GetSessionModel)okResult.Value;

                Assert.Equal(session.SessionId, getSession.SessionId);
                var checkToParent = dbContext.Sessions
                    .Include(x => x.SubSessions)
                    .Single(x => x.SessionId == toParent.SessionId);
                Assert.Contains(checkToParent.SubSessions, x => x.SessionId == session.SessionId);
            }
        }

        [Fact]
        public async void SessionMoveScheduleWrongLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var fromLeague = Fixture.Leagues.First();
                var toLeague = Fixture.Leagues.ElementAt(1);
                var season = fromLeague.Seasons.ElementAt(1);
                var fromSchedule = season.Schedules.ElementAt(0);
                var toSchedule = toLeague.Seasons.First().Schedules.First();
                var session = fromSchedule.Sessions.First();

                var putSession = new PutSessionModel()
                {
                    SessionId = session.SessionId,
                    ScheduleId = toSchedule.ScheduleId,
                    Name = session.Name,
                    Date = session.Date
                };

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Put(fromLeague.Name, fromLeague.Id, putSession)).Result;
                Assert.IsNotType<OkObjectResult>(result);

                var checkFromSchedule = dbContext.Schedules.Single(x => x.ScheduleId == fromSchedule.ScheduleId);
                var checkToSchedule = dbContext.Schedules.Single(x => x.ScheduleId == toSchedule.ScheduleId);
                Assert.Contains(checkFromSchedule.Sessions, x => x.SessionId == session.SessionId);
                Assert.DoesNotContain(checkToSchedule.Sessions, x => x.SessionId == session.SessionId);
            }
        }

        [Fact]
        public async void DeleteSession()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const int testLeagueId = 1;
                const int testSessionId = 1;

                var controller = AddContexts.AddMemberControllerContext(new SessionsController(MockLogger, dbContext));
                var result = (await controller.Delete(testLeagueName, testLeagueId, testSessionId));

                Assert.IsType<OkResult>(result);
                Assert.DoesNotContain(dbContext.Sessions, x => x.SessionId == testSessionId);
            }
        }
    }
}
