using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Schedules
{
    [Collection("HandlerTests")]
    public class DeleteScheduleHandlerTests : HandlersTestsBase<DeleteScheduleHandler, DeleteScheduleRequest, Unit>
    {
        public DeleteScheduleHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeleteScheduleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteScheduleRequest> validator)
        {
            return new DeleteScheduleHandler(logger, dbContext, new IValidator<DeleteScheduleRequest>[] { validator });
        }

        protected override DeleteScheduleRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScheduleId);
        }

        private DeleteScheduleRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
        {
            return new DeleteScheduleRequest(leagueId, scheduleId);
        }

        protected override void DefaultPreTestAssertions(DeleteScheduleRequest request, LeagueDbContext dbContext)
        {
            // assert schedule exists in dbContext
            Assert.Contains(dbContext.Schedules, x => x.ScheduleId == request.ScheduleId);
        }

        protected override void DefaultAssertions(DeleteScheduleRequest request, Unit result, LeagueDbContext dbContext)
        {
            // assert schedule was deleted
            Assert.DoesNotContain(dbContext.Schedules, x => x.ScheduleId == request.ScheduleId);
        }

        [Fact]
        public override async Task<Unit> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
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
