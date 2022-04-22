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
    public class PostSessionToParentSessionHandlerTests : HandlersTestsBase<PostSessionToParentSessionHandler, PostSessionToParentSessionRequest, GetSessionModel>
    {
        public PostSessionToParentSessionHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostSessionToParentSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostSessionToParentSessionRequest> validator)
        {
            return new PostSessionToParentSessionHandler(logger, dbContext, new IValidator<PostSessionToParentSessionRequest>[] { validator });
        }

        protected override PostSessionToParentSessionRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private PostSessionToParentSessionRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
        {
            var model = new PostSessionModel()
            {
            };
            return new PostSessionToParentSessionRequest(leagueId, scheduleId, DefaultUser(), model);
        }

        protected override void DefaultAssertions(PostSessionToParentSessionRequest request, GetSessionModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            // assert collection
            var parent = dbContext.Sessions
                .Include(x => x.SubSessions)
                .SingleOrDefault(x => x.SessionId == request.ParentSessionId);
            Assert.Contains(parent.SubSessions, x => x.SessionId == result.SessionId);
            // assert creation metadata
            var testTime = DateTime.UtcNow;
            Assert.Equal(testTime, result.CreatedOn.GetValueOrDefault(), TimeSpan.FromMinutes(1));
            Assert.Equal(testUserId, result.CreatedByUserId);
            Assert.Equal(testUserName, result.CreatedByUserName);
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
