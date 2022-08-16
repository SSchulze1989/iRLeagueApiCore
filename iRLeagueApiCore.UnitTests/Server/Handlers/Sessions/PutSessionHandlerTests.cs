using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Sessions
{
    [Collection("HandlerTests")]
    public class PutSessionHandlerTests : HandlersTestsBase<PutSessionHandler, PutSessionRequest, SessionModel>
    {
        private const long testSessionId = 1;
        private const string testSessionName = "TestSession";
        private const string testSessionTitle = "TestTitle";
        private const string testSubSessionName = "TestSubSession";
        private static readonly DateTime testSessionDate = DateTime.Today;
        private const SessionType testSessionType = SessionType.Heat;
        private const SimSessionType testSimSessionType = SimSessionType.Race;
        private const long testTrackId = 5;
        private static readonly TimeSpan testSessionDuration = TimeSpan.FromMinutes(1.23);
        private const int testLaps = 12;
        private const bool testAttached = true;
        private const int testSubSessionNr = 2;

        public PutSessionHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PutSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutSessionRequest> validator)
        {
            return new PutSessionHandler(logger, dbContext, new IValidator<PutSessionRequest>[] { validator });
        }

        protected override PutSessionRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        protected override void DefaultAssertions(PutSessionRequest request, SessionModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var testTime = DateTime.UtcNow;
            Assert.Equal(testSessionId, result.SessionId);
            Assert.Equal(testSessionName, result.Name);
            Assert.Equal(testSessionDate, result.Date);
            Assert.Equal(testSessionType, result.SessionType);
            Assert.Equal(testTrackId, result.TrackId);
            Assert.Equal(testSessionDuration, result.Duration);
            Assert.Equal(testTime, result.LastModifiedOn.GetValueOrDefault(), TimeSpan.FromMinutes(1));
            Assert.Equal(request.User.Id, result.LastModifiedByUserId);
            Assert.Equal(request.User.Name, result.LastModifiedByUserName);
            var subSession = result.SubSessions.FirstOrDefault();
            Assert.NotNull(subSession);
            Assert.Equal(testSubSessionName, subSession.Name);
            Assert.Equal(testSimSessionType, subSession.SessionType);
            Assert.Equal(testSubSessionNr, subSession.SubSessionNr);
        }

        private PutSessionRequest DefaultRequest(long leagueId = testLeagueId, long sessionId = testSessionId)
        {
            var model = new PutSessionModel()
            {
                Name = testSessionName,
                SessionType = testSessionType,
                Date = testSessionDate,
                TrackId = testTrackId,
                Duration = testSessionDuration,
                SubSessions = new List<PutSessionSubSessionModel>()
                {
                    new PutSessionSubSessionModel()
                    {
                        Name = testSubSessionName,
                        SessionType = testSimSessionType,
                        SubSessionNr = testSubSessionNr,
                    }
                },
            };
            return new PutSessionRequest(leagueId, DefaultUser(), sessionId, model);
        }

        [Fact]
        public async override Task<SessionModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public async override Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, testSessionId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testSessionId)]
        [InlineData(testLeagueId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long sessionId)
        {
            var request = DefaultRequest(leagueId, sessionId);
            await HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public async Task ShouldMapSubSessions()
        {
            using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            long removeSubSessionId;

            using (var context = DbTestFixture.CreateStaticDbContext())
            { 
                var subSession1 = new PutSessionSubSessionModel()
                {
                    Name = "SubSession1",
                    SubSessionNr = 1,
                };
                var subSession2 = new PutSessionSubSessionModel()
                {
                    Name = "SubSession2",
                    SubSessionNr = 2,
                };
                var session = new PutSessionModel()
                {
                    SubSessions = new List<PutSessionSubSessionModel>() { subSession1, subSession2 },
                };
                var sessionEntity = context.Sessions
                    .Include(x => x.SubSessions)
                    .First();
                removeSubSessionId = sessionEntity.SubSessions.First().SubSessionId;

                var request = new PutSessionRequest(sessionEntity.LeagueId, DefaultUser(), sessionEntity.SessionId, session);
                var handler = CreateTestHandler(context, MockHelpers.TestValidator<PutSessionRequest>());
                await handler.Handle(request, default);
            }

            using (var context = DbTestFixture.CreateStaticDbContext())
            {
                var sessionEntity = context.Sessions
                    .Include(x => x.SubSessions)
                    .First();
                Assert.Equal(2, sessionEntity.SubSessions.Count);
                Assert.Equal("SubSession1", sessionEntity.SubSessions.ElementAt(0).Name);
                Assert.Equal("SubSession2", sessionEntity.SubSessions.ElementAt(1).Name);
                Assert.DoesNotContain(context.SubSessions, x => x.SubSessionId == removeSubSessionId);
            }
        }

        [Fact]
        public async Task ShouldMapWithExistingSubSessions()
        {
            using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            long updateSubSessionId;

            using (var context = DbTestFixture.CreateStaticDbContext())
            {
                var sessionEntity = context.Sessions
                    .Include(x => x.SubSessions)
                    .First();
                updateSubSessionId = sessionEntity.SubSessions.First().SessionId;
                var subSession1 = new PutSessionSubSessionModel()
                {
                    Name = "SubSession1",
                    SubSessionNr = 1,
                };
                var subSession2 = new PutSessionSubSessionModel()
                {
                    Name = "Updated Race",
                    SubSessionId = updateSubSessionId,
                    SubSessionNr = 2,
                };
                var session = new PutSessionModel()
                {
                    SubSessions = new List<PutSessionSubSessionModel>() { subSession1, subSession2 },
                };

                var request = new PutSessionRequest(sessionEntity.LeagueId, DefaultUser(), sessionEntity.SessionId, session);
                var handler = CreateTestHandler(context, MockHelpers.TestValidator<PutSessionRequest>());
                await handler.Handle(request, default);
            }

            using (var context = DbTestFixture.CreateStaticDbContext())
            {
                var sessionEntity = context.Sessions
                    .Include(x => x.SubSessions)
                    .First();
                Assert.Equal(2, sessionEntity.SubSessions.Count);
                Assert.Equal(updateSubSessionId, sessionEntity.SubSessions.ElementAt(0).SubSessionId);
                Assert.Equal("Updated Race", sessionEntity.SubSessions.ElementAt(0).Name);
                Assert.Equal("SubSession1", sessionEntity.SubSessions.ElementAt(1).Name);
            }
        }
    }
}
