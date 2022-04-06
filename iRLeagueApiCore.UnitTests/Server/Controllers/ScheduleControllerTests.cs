using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    [Collection("ControllerTests")]
    public class ScheduleControllerTests : IClassFixture<DbTestFixture>
    {
        DbTestFixture Fixture { get; }
        ITestOutputHelper Output { get; }
        ILogger<SchedulesController> MockLogger { get; }

        public ScheduleControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            MockLogger = new Mock<ILogger<SchedulesController>>().Object;
        }

        [Fact]
        public async void GetSchedule()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const long testScheduleId = 1;
                const string testScheduleName = "S1 Schedule";

                var controller = AddContexts.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var result = (await controller.Get(testLeagueName, testLeagueId, new long[] { testScheduleId })).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (IEnumerable<GetScheduleModel>)okResult.Value;
                var schedule = resultValue.FirstOrDefault();

                Assert.NotNull(schedule);
                Assert.Equal(testScheduleId, schedule.ScheduleId);
                Assert.Equal(testScheduleName, schedule.Name);
            }
        }

        [Fact]
        public async void CreateSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const string testScheduleName = "S1 Schedule 2";
                const long testSeasonId = 1;

                var controller = AddContexts.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var putSchedule = new PutScheduleModel()
                {
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, testLeagueId, testSeasonId, putSchedule)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSchedule = (GetScheduleModel)okResult?.Value;

                Assert.NotNull(getSchedule);
                Assert.Equal(testScheduleName, getSchedule.Name);
                Assert.Equal(testSeasonId, getSchedule.SeasonId);
                Assert.Equal(testLeagueId, getSchedule.LeagueId);
            }
        }

        [Fact]
        public async Task DeleteSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {

                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const long testScheduleId = 1;

                var controller = AddContexts.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var result = (await controller.Delete(testLeagueName, testLeagueId, testScheduleId));

                Assert.IsType<NoContentResult>(result);
                Assert.DoesNotContain(dbContext.Schedules, x => x.ScheduleId == testScheduleId);
            }
        }
    }
}
