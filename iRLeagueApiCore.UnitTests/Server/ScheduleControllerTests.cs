using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ScheduleControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
        }

        [Fact]
        public async void TestGetSchedule()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testScheduleId = 1;
                const string testScheduleName = "S1 Schedule";

                var controller = Fixture.AddControllerContext(new ScheduleController());
                var result = (await controller.Get(testLeagueName, new long[] { testScheduleId }, dbContext)).Result;
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
        public async void TestGetScheduleForbidden()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague1";
                const long testScheduleId = 1;

                var controller = Fixture.AddControllerContextWithoutLeagueRole(new ScheduleController());
                var result = (await controller.Get(testLeagueName, new long[] { testScheduleId }, dbContext)).Result;
                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async void TestCreateSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague";
                const long testLeagueId = 1;
                const string testScheduleName = "S1 Schedule 2";
                const long testSeasonId = 1;

                var controller = Fixture.AddControllerContext(new ScheduleController());
                var putSchedule = new PutScheduleModel()
                {
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, putSchedule, dbContext)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var getSchedule = (GetScheduleModel)okResult?.Value;

                Assert.NotNull(getSchedule);
                Assert.Equal(testScheduleName, getSchedule.Name);
                Assert.Equal(testSeasonId, getSchedule.SeasonId);
                Assert.Equal(testLeagueId, getSchedule.leagueId);
            }
        }

        [Fact]
        public async Task TestCreateWrongLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague2";
                const string testScheduleName = "L2S1 Schedule 1";
                const long testSeasonId = 1;

                var controller = Fixture.AddControllerContext(new ScheduleController());
                var putSchedule = new PutScheduleModel()
                {
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, putSchedule, dbContext)).Result;
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
        public async void TestUpdateSchedule()
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

                var controller = Fixture.AddControllerContext(new ScheduleController());
                var putSchedule = new PutScheduleModel()
                {
                    ScheduleId = testScheduleId,
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, putSchedule, dbContext)).Result;
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
                Assert.Equal(testLeagueId, getSchedule.leagueId);
                Assert.Equal(expectSchedulesCount, schedulesCount);
            }
        }

        [Fact]
        public async void TestChangeSeason()
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

                var controller = Fixture.AddControllerContext(new ScheduleController());
                var putSchedule = new PutScheduleModel()
                {
                    ScheduleId = testScheduleId,
                    SeasonId = testSeasonId,
                    Name = testScheduleName
                };
                var result = (await controller.Put(testLeagueName, putSchedule, dbContext)).Result;
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
                Assert.Equal(testLeagueId, getSchedule.leagueId);
                Assert.Equal(expectSchedulesCount, schedulesCount);
            }
        }

        [Fact]
        public async Task TestUpdateScheduleForbidden()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                const string testLeagueName = "TestLeague1";
                const long testScheduleId = 1;

                var controller = Fixture.AddControllerContextWithoutLeagueRole(new ScheduleController());
                var putSchedule = new PutScheduleModel()
                {
                    ScheduleId = testScheduleId,
                    Name = "Forbidden update"
                };

                var result = (await controller.Put(testLeagueName, putSchedule, dbContext)).Result;
                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async Task TestDeleteSchedule()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {

                const string testLeagueName = "TestLeague";
                const long testScheduleId = 1;

                var controller = Fixture.AddControllerContext(new ScheduleController());
                var result = (await controller.Delete(testLeagueName, testScheduleId, dbContext));

                Assert.IsType<OkResult>(result);
                Assert.DoesNotContain(dbContext.Schedules, x => x.ScheduleId == testScheduleId);
            }
        }
    }
}
