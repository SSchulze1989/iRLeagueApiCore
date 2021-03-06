using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Controllers
{
    [Collection("ControllerTests")]
    public class QueriesTests : IClassFixture<DbTestFixture>
    {
        private DbTestFixture Fixture { get; }
        public QueriesTests(DbTestFixture fixture)
        {
            Fixture = fixture;
        }

        /// <summary>
        /// Load all required data for the result through a single query
        /// </summary>
        [Fact]
        public async void GetResultQuery()
        {
            using (var context = Fixture.CreateDbContext())
            {
                long testSeasonId = (await context.Seasons
                    .FirstAsync())
                    .SeasonId;

                // query results by season id
                var query = context.ScoredResults
                    .Select(result => new ResultModel()
                    {
                        LeagueId = result.LeagueId,
                        SeasonId = result.Result.Session.Schedule.SeasonId,
                        SessionId = result.ResultId,
                        ResultRows = result.ScoredResultRows
                            .Select(row => new ResultRowModel()
                            {
                                Firstname = row.Member.Firstname,
                                Lastname = row.Member.Lastname,
                                TeamName = row.Team.Name
                            }).ToArray(),
                    })
                    .Where(x => x.SeasonId == testSeasonId);
                var sql = query.ToQueryString();
                var results = await query.ToListAsync();

                Assert.NotNull(results);
                var result = results.FirstOrDefault();
                Assert.NotNull(result);
                Assert.Equal(1, result.LeagueId);
                Assert.Equal(1, result.SeasonId);
                Assert.Equal(1, result.SessionId);
                Assert.Equal(10, result.ResultRows.Count());
                var row = result.ResultRows.First();
                Assert.NotNull(row.Firstname);
                Assert.NotNull(row.Lastname);
            }
        }
    }
}
