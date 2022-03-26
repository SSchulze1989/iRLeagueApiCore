using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server
{
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

                var controller = Fixture.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
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

                var controller = Fixture.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var putSchedule = new PutScheduleModel()
                {
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, testLeagueId, putSchedule)).Result;
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
        public async Task CreateWrongLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague2";
                const long testLeagueId = 2;
                const string testScheduleName = "L2S1 Schedule 1";
                const long testSeasonId = 1;

                var controller = Fixture.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var putSchedule = new PutScheduleModel()
                {
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, testLeagueId, putSchedule)).Result;
                Assert.IsNotType<OkObjectResult>(result);
                Assert.IsNotType<OkResult>(result);

                // check that schedule was not added
                var testSeason = dbContext.Seasons
                    .Where(x => x.SeasonId == testSeasonId)
                    .Single();
                Assert.DoesNotContain(testSeason.Schedules, x => x.Name == testScheduleName);
            }
        }

        [Fact]
        public async void UpdateSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const long testScheduleId = 1;
                const string testScheduleName = "S1 Schedule 1 Mod";
                const long testSeasonId = 1;

                // count schedules before update
                var expectSchedulesCount = dbContext.Seasons
                    .Where(x => x.SeasonId == testSeasonId)
                    .Select(x => x.Schedules.Count)
                    .First();

                var controller = Fixture.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var putSchedule = new PutScheduleModel()
                {
                    ScheduleId = testScheduleId,
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, testLeagueId, putSchedule)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSchedule = (GetScheduleModel)okResult?.Value;

                var schedulesCount = dbContext.Seasons
                    .Where(x => x.SeasonId == testSeasonId)
                    .Select(x => x.Schedules.Count)
                    .First();

                Assert.NotNull(getSchedule);
                Assert.Equal(testScheduleName, getSchedule.Name);
                Assert.Equal(testSeasonId, getSchedule.SeasonId);
                Assert.Equal(testLeagueId, getSchedule.LeagueId);
                Assert.Equal(expectSchedulesCount, schedulesCount);
            }
        }

        [Fact]
        public async void ChangeSeason()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const long testScheduleId = 1;
                const string testScheduleName = "S1 Schedule 1 Mod";
                const long testSeasonId = 2;

                // count schedules before update
                var expectSchedulesCount = dbContext.Seasons
                    .Where(x => x.SeasonId == testSeasonId)
                    .Select(x => x.Schedules.Count)
                    .First() + 1;

                var controller = Fixture.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var putSchedule = new PutScheduleModel()
                {
                    ScheduleId = testScheduleId,
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, testLeagueId, putSchedule)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSchedule = (GetScheduleModel)okResult?.Value;

                var schedulesCount = dbContext.Seasons
                    .Where(x => x.SeasonId == testSeasonId)
                    .Select(x => x.Schedules.Count)
                    .First();

                Assert.NotNull(getSchedule);
                Assert.Equal(testScheduleName, getSchedule.Name);
                Assert.Equal(testSeasonId, getSchedule.SeasonId);
                Assert.Equal(testLeagueId, getSchedule.LeagueId);
                Assert.Equal(expectSchedulesCount, schedulesCount);
            }
        }

        [Fact]
        public async Task UpdateScheduleForbidden()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague1";
                const long testLeagueId = 1;
                const long testScheduleId = 1;

                var controller = Fixture.AddControllerContextWithoutLeagueRole(new SchedulesController(MockLogger, dbContext));
                var putSchedule = new PutScheduleModel()
                {
                    ScheduleId = testScheduleId,
                    Name = "Forbidden update"
                };

                var result = (await controller.Put(testLeagueName, testLeagueId, putSchedule)).Result;
                Assert.IsType<BadRequestObjectResult>(result);
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

                var controller = Fixture.AddMemberControllerContext(new SchedulesController(MockLogger, dbContext));
                var result = (await controller.Delete(testLeagueName, testLeagueId, testScheduleId));

                Assert.IsType<NoContentResult>(result);
                Assert.DoesNotContain(dbContext.Schedules, x => x.ScheduleId == testScheduleId);
            }
        }
    }
}
