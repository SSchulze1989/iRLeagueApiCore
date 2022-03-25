using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server
{
    public class LeagueControllerTests : IClassFixture<DbTestFixture>
    {
        DbTestFixture Fixture { get; }
        ITestOutputHelper Output { get; }

        public LeagueControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task GetLeague()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = new LeaguesController();
                var result = (await controller.Get(new long[] { 1 }, dbContext)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (IEnumerable<GetLeagueModel>)okResult.Value;
                Assert.Single(resultValue);
                Assert.Equal(1, resultValue.First().LeagueId);
            }
        }

        [Fact]
        public async Task CreateLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                const int testLeagueId = 3;
                var controller = Fixture.AddMemberControllerContext(new LeaguesController());

                var putLeague = new PutLeagueModel()
                {
                    LeagueId = 0,
                    Name = "UnitTestLeague",
                    NameFull = "League for unit testing"
                };
                var result = (await controller.Put(putLeague, dbContext)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (GetLeagueModel)okResult.Value;
                Assert.Equal(testLeagueId, resultValue.LeagueId);
            }
        }

        [Fact]
        public async Task UpdateLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = Fixture.AddMemberControllerContext(new LeaguesController());

                var putLeague = new PutLeagueModel()
                {
                    LeagueId = 1,
                    Name = "New-League_Name123",
                    NameFull = "League name after test"
                };

                // make sure league exists and data is different
                var checkLeague = dbContext.Find<LeagueEntity>(putLeague.LeagueId);
                Assert.NotEqual(putLeague.Name, checkLeague.Name);
                Assert.NotEqual(putLeague.NameFull, checkLeague.NameFull);

                var result = (await controller.Put(putLeague, dbContext)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (GetLeagueModel)okResult.Value;
                Assert.Equal(1, resultValue.LeagueId);

                // check if first league was updated
                checkLeague = dbContext.Find<LeagueEntity>(putLeague.LeagueId);
                Assert.Equal(putLeague.Name, checkLeague.Name);
                Assert.Equal(putLeague.NameFull, checkLeague.NameFull);
            }
        }

        [Fact]
        public async Task CreateInvalidName()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = Fixture.AddMemberControllerContext(new LeaguesController());

                var putLeague = new PutLeagueModel()
                {
                    LeagueId = 0,
                    Name = "Name with spaces",
                    NameFull = "League for unit testing"
                };
                var result = (await controller.Put(putLeague, dbContext)).Result;

                Assert.IsNotType<OkObjectResult>(result);
                Assert.IsNotType<OkResult>(result);
                Assert.Null(dbContext.Leagues.SingleOrDefault(x => x.Name == putLeague.Name));
            }
        }
    }
}
