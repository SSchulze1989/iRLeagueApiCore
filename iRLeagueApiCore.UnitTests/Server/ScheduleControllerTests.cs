using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
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
        public async void TestPutSchedule()
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
    }
}
