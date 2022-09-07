using FluentValidation;
using iRLeagueApiCore.Common.Models;
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
    [Collection("HandlerTests")]
    public class GetScheduleHandlerTests : HandlersTestsBase<GetScheduleHandler, GetScheduleRequest, ScheduleModel>
    {
        public GetScheduleHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScheduleRequest> validator)
        {
            return new GetScheduleHandler(logger, dbContext, new IValidator<GetScheduleRequest>[] { validator });
        }

        protected override GetScheduleRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScheduleId);
        }

        private GetScheduleRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
        {
            return new GetScheduleRequest(leagueId, scheduleId);
        }

        protected override void DefaultAssertions(GetScheduleRequest request, ScheduleModel result, LeagueDbContext dbContext)
        {
            Assert.Equal(request.LeagueId, result.LeagueId);
            Assert.Equal(request.ScheduleId, result.ScheduleId);
        }

        [Fact]
        public override async Task<ScheduleModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }

        [Theory]
        [InlineData(0, testScheduleId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(43, testScheduleId)]
        [InlineData(testLeagueId, 43)]
        public async Task HandleNotFoundAsync(long leagueId, long scheduleId)
        {
            var request = DefaultRequest(leagueId, scheduleId);
            await base.HandleNotFoundRequestAsync(request);
        }
    }
}
