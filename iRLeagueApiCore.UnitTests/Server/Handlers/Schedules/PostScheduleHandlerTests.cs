using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Schedules
{
    public class PostScheduleHandlerTests : HandlersTestsBase<PostScheduleHandler, PostScheduleRequest, GetScheduleModel>
    {
        private const string testScheduleName = "TestSchedule";

        public PostScheduleHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostScheduleRequest> validator)
        {
            return new PostScheduleHandler(logger, dbContext, new IValidator<PostScheduleRequest>[] { validator });
        }

        protected override PostScheduleRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testSeasonId);
        }

        private PostScheduleRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            var model = new PostScheduleModel()
            {
                Name = testScheduleName
            };
            return new PostScheduleRequest(leagueId, seasonId, DefaultUser(), model);
        }

        protected override void DefaultAssertions(PostScheduleRequest request, GetScheduleModel result, LeagueDbContext dbContext)
        {
            Assert.Equal(request.LeagueId, result.LeagueId);
            Assert.NotEqual(0, result.ScheduleId);
            Assert.Equal(testUserId, result.CreatedByUserId);
            Assert.Equal(testUserName, result.CreatedByUserName);
            Assert.Equal(DateTime.UtcNow, result.CreatedOn.GetValueOrDefault(), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public override async Task<GetScheduleModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, testSeasonId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(43, testSeasonId)]
        [InlineData(testLeagueId, 43)]
        public async Task HandleNotFoundAsync(long leagueId, long seasonId  )
        {
            var request = DefaultRequest(leagueId, seasonId);
            await base.HandleNotFoundRequestAsync(request);
        }
    }
}
