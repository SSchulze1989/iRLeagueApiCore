using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Sessions
{
    [Collection("HandlerTests")]
    public class PostSessionToScheduleHandlerTests : HandlersTestsBase<PostSessionToScheduleHandler, PostSessionToScheduleRequest, SessionModel>
    {
        public PostSessionToScheduleHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostSessionToScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostSessionToScheduleRequest> validator)
        {
            return new PostSessionToScheduleHandler(logger, dbContext, new IValidator<PostSessionToScheduleRequest>[] { validator });
        }

        protected override PostSessionToScheduleRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private PostSessionToScheduleRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
        {
            var model = new PostSessionModel()
            {
                SubSessions = new List<PutSessionSubSessionModel>(),
            };
            return new PostSessionToScheduleRequest(leagueId, scheduleId, DefaultUser(), model);
        }

        protected override void DefaultAssertions(PostSessionToScheduleRequest request, SessionModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            // assert collection
            var schedule = dbContext.Schedules
                .Include(x => x.Sessions)
                .SingleOrDefault(x => x.ScheduleId == request.ScheduleId);
            Assert.Contains(schedule.Sessions, x => x.SessionId == result.SessionId);
            // assert creation metadata
            var testTime = DateTime.UtcNow;
            Assert.Equal(testTime, result.CreatedOn.GetValueOrDefault(), TimeSpan.FromMinutes(1));
            Assert.Equal(testUserId, result.CreatedByUserId);
            Assert.Equal(testUserName, result.CreatedByUserName);
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
        [InlineData(0, testScheduleId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testScheduleId)]
        [InlineData(testLeagueId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long scheduleId)
        {
            var request = DefaultRequest(leagueId, scheduleId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
