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
    public class GetSchedulesHandlerTests : HandlersTestsBase<GetSchedulesHandler, GetSchedulesRequest, IEnumerable<GetScheduleModel>>
    {
        public GetSchedulesHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetSchedulesHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSchedulesRequest> validator)
        {
            return new GetSchedulesHandler(logger, dbContext, new IValidator<GetSchedulesRequest>[] { validator });
        }

        protected override GetSchedulesRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetSchedulesRequest DefaultRequest(long leagueId)
        {
            return new GetSchedulesRequest(leagueId);
        }

        protected override void DefaultAssertions(GetSchedulesRequest request, IEnumerable<GetScheduleModel> result, LeagueDbContext dbContext)
        {
            foreach(var dbSchedule in dbContext.Schedules.Where(x => x.LeagueId == testLeagueId))
            {
                Assert.Contains(result, x => x.ScheduleId == dbSchedule.ScheduleId);
            }
        }

        [Fact]
        public override async Task<IEnumerable<GetScheduleModel>> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(43)]
        public async Task HandleNotFoundAsync(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await base.HandleNotFoundRequestAsync(request);
        }
    }
}
