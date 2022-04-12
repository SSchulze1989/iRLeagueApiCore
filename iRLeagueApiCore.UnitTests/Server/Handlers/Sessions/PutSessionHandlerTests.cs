using FluentValidation;
using iRLeagueApiCore.Communication.Enums;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Sessions
{
    [Collection("HandlerTests")]
    public class PutSessionHandlerTests : HandlersTestsBase<PutSessionHandler, PutSessionRequest, GetSessionModel>
    {
        private const long testSessionId = 1;
        private const string testSessionName = "TestSession";
        private const string testSessionTitle = "TestTitle";
        private static readonly DateTime testSessionDate = DateTime.Today;
        private const SessionType testSessionType = SessionType.Heat;
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

        protected override void DefaultAssertions(PutSessionRequest request, GetSessionModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var testTime = DateTime.UtcNow;
            Assert.Equal(testSessionId, result.SessionId);
            Assert.Equal(testSessionTitle, result.SessionTitle);
            Assert.Equal(testSessionName, result.Name);
            Assert.Equal(testSessionDate, result.Date);
            Assert.Equal(testSessionType, result.SessionType);
            Assert.Equal(testTrackId, result.TrackId);
            Assert.Equal(testSessionDuration, result.Duration);
            Assert.Equal(testSessionDuration, result.PracticeLength);
            Assert.Equal(testSessionDuration, result.QualyLength);
            Assert.Equal(testSessionDuration, result.RaceLength);
            Assert.Equal(testAttached, result.QualyAttached);
            Assert.Equal(testAttached, result.PracticeAttached);
            Assert.Equal(testSubSessionNr, result.SubSessionNr);
            Assert.Equal(testTime, result.LastModifiedOn.GetValueOrDefault(), TimeSpan.FromMinutes(1));
            Assert.Equal(request.User.Id, result.LastModifiedByUserId);
            Assert.Equal(request.User.Name, result.LastModifiedByUserName);
        }

        private PutSessionRequest DefaultRequest(long leagueId = testLeagueId, long sessionId = testSessionId)
        {
            var model = new PutSessionModel()
            {
                SessionTitle = testSessionTitle,
                Name = testSessionName,
                SessionType = testSessionType,
                Date = testSessionDate,
                TrackId = testTrackId,
                Duration = testSessionDuration,
                Laps = testLaps,
                PracticeLength = testSessionDuration,
                QualyLength = testSessionDuration,
                RaceLength = testSessionDuration,
                QualyAttached = testAttached,
                PracticeAttached = testAttached,
                SubSessionNr = testSubSessionNr,
            };
            return new PutSessionRequest(leagueId, DefaultUser(), sessionId, model);
        }

        [Fact]
        public async override Task<GetSessionModel> HandleDefaultAsync()
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
    }
}
