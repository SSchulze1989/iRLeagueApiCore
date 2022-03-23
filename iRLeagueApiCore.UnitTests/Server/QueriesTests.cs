using iRLeagueApiCore.Communication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server
{
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
        public async void TestGetResultQuery()
        {
            using (var context = Fixture.CreateDbContext())
            {
                const long testSeasonId = 1;

                // query results by season id
                var query = context.ScoredResults
                    .Select(result => new GetResultModel()
                    {
                        LeagueId = result.LeagueId,
                        SeasonId = result.Result.Session.Schedule.SeasonId,
                        SessionId = result.ResultId,
                        ResultRows = result.ScoredResultRows
                            .Select(row => new GetResultRow()
                            {
                                Firstname = row.ResultRow.Member.Firstname,
                                Lastname = row.ResultRow.Member.Lastname,
                                TeamName = row.ResultRow.Team.Name
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
