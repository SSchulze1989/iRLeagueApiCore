using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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
        ILogger<LeaguesController> MockLogger { get; }

        public LeagueControllerTests(DbTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            MockLogger = new Mock<ILogger<LeaguesController>>().Object;
        }

        [Fact]
        public async Task GetLeague()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = Fixture.AddMemberControllerContext(new LeaguesController(MockLogger, dbContext));
                var result = (await controller.Get(new long[] { 1 })).Result;
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
                const string testLeagueName = "UnitTestLeague";
                var controller = Fixture.AddMemberControllerContext(new LeaguesController(MockLogger, dbContext));

                var putLeague = new PutLeagueModel()
                {
                    LeagueId = 0,
                    Name = testLeagueName,
                    NameFull = "League for unit testing"
                };
                var result = (await controller.Put(putLeague)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (GetLeagueModel)okResult.Value;
                Assert.NotEqual(0, resultValue.LeagueId);
                Assert.Equal(testLeagueName, resultValue.Name);
            }
        }

        [Fact]
        public async Task UpdateLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = Fixture.AddMemberControllerContext(new LeaguesController(MockLogger, dbContext));

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

                var result = (await controller.Put(putLeague)).Result;
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
                var controller = Fixture.AddMemberControllerContext(new LeaguesController(MockLogger, dbContext));

                var putLeague = new PutLeagueModel()
                {
                    LeagueId = 0,
                    Name = "Name with spaces",
                    NameFull = "League for unit testing"
                };
                var result = (await controller.Put(putLeague)).Result;

                Assert.IsNotType<OkObjectResult>(result);
                Assert.IsNotType<OkResult>(result);
                Assert.Null(dbContext.Leagues.SingleOrDefault(x => x.Name == putLeague.Name));
            }
        }

        [Fact]
        public async Task CreateLeagueNameExists()
        {
            const string testLeagueName = "tEstlEague"; // Test with an existing name but change upper case letters

            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = Fixture.AddAdminControllerContext(new LeaguesController(MockLogger, dbContext));

                var putLeague = new PutLeagueModel()
                {
                    Name = testLeagueName,
                    NameFull = "League with existing name should fail"
                };
                var result = (await controller.Put(putLeague));

                var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
                var response = Assert.IsType<ResultResponse>(badRequest.Value);
                Assert.Equal("League exists", response.Result);
            }
        }
    }
}
