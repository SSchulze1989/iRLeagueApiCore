using FluentValidation;
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
    public class GetSessionHandlerTests : HandlersTestsBase<GetSessionHandler, GetSessionRequest, GetSessionModel>
    {
        private const long testSessionId = 1;

        public GetSessionHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSessionRequest> validator)
        {
            return new GetSessionHandler(logger, dbContext, new IValidator<GetSessionRequest>[] { validator });
        }

        protected override GetSessionRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetSessionRequest DefaultRequest(long leagueId = testLeagueId, long sessionId = testSessionId)
        {
            return new GetSessionRequest(leagueId, sessionId);
        }

        protected override void DefaultAssertions(GetSessionRequest request, GetSessionModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            // Assert properties are mapped
            var entity = dbContext.Sessions
                .Single(x => x.SessionId == request.SessionId);
            Assert.Equal(entity.CreatedByUserId, result.CreatedByUserId);
            Assert.Equal(entity.CreatedByUserName, result.CreatedByUserName);
            Assert.Equal(entity.CreatedOn, result.CreatedOn);
            Assert.Equal(entity.Date, result.Date);
            Assert.Equal(entity.Duration, result.Duration);
            Assert.Equal(entity.Laps.GetValueOrDefault(), result.Laps);
            Assert.Equal(entity.LastModifiedByUserId, result.LastModifiedByUserId);
            Assert.Equal(entity.LastModifiedByUserName, result.LastModifiedByUserName);
            Assert.Equal(entity.LastModifiedOn, result.LastModifiedOn);
            Assert.Equal(entity.LeagueId, result.LeagueId);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.PracticeAttached.GetValueOrDefault(), result.PracticeAttached);
            Assert.Equal(entity.PracticeLength, result.PracticeLength);
            Assert.Equal(entity.QualyAttached.GetValueOrDefault(), result.QualyAttached);
            Assert.Equal(entity.QualyLength, result.QualyLength);
            Assert.Equal(entity.RaceLength, result.RaceLength);
            Assert.Equal(entity.ScheduleId, result.ScheduleId);
            Assert.Equal(entity.SessionId, result.SessionId);
            Assert.Equal(entity.SessionTitle, result.SessionTitle);
            Assert.Equal(entity.SessionType, result.SessionType);
            Assert.Equal(entity.SubSessionNr, result.SubSessionNr);
            Assert.Equal(entity.TrackId, result.TrackId);
        }

        [Fact]
        public async override Task<GetSessionModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 42)]
        [InlineData(42, 1)]
        public async Task HandleNotFound(long leagueId, long sessionId)
        {
            var request = DefaultRequest(leagueId, sessionId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
