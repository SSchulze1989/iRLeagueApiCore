using DbIntegrationTests;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace iRLeagueApiCore.UnitTests.Server
{
    public class DbTestFixture : IDisposable
    {
        private IConfiguration Configuration { get; set; }

        private static readonly int Seed = 12345;

        public DbTestFixture()
        {
            Configuration = ((IConfigurationBuilder)(new ConfigurationBuilder()))
                .AddUserSecrets<DbTestFixture>()
                .Build(); ;

            var random = new Random(Seed);

            // set up test database
            using (var dbContext = CreateDbContext())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                PopulateTestDatabase.Populate(dbContext, random);
                dbContext.SaveChanges();
            }
        }

        public LeagueDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
            var connectionString = Configuration["Db:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "server=localhost;user=testuser;password=testuserpw;database=TestDatabase;";
            }

            optionsBuilder.UseMySQL(Configuration["Db:ConnectionString"]);
            var dbContext = new LeagueDbContext(optionsBuilder.Options);
            return dbContext;
        }

        public void Dispose()
        {
        }
    }

    public class LeagueControllerTests : IClassFixture<DbTestFixture>
    {
        DbTestFixture Fixture { get; }

        public LeagueControllerTests(DbTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task TestGetLeague()
        {
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = new LeagueController();
                var result = (await controller.Get(new long[] { 1 }, dbContext)).Result;
                Assert.IsType<OkObjectResult>(result);
                var okResult = (OkObjectResult)result;
                var resultValue = (IEnumerable<GetLeagueModel>)okResult.Value;
                Assert.Single(resultValue);
                Assert.Equal(1, resultValue.First().LeagueId);
            }
        }

        [Fact]
        public async Task TestCreatLeague()
        {
            using (var tx = new TransactionScope())
            using (var dbContext = Fixture.CreateDbContext())
            {
                var controller = new LeagueController();

                var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, "unitTestUser"),
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim("custom-claim", "example claim value"),
                }, "mock"));

                controller.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                };
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
                Assert.Equal(2, resultValue.LeagueId);
            }
        }
    }
}
