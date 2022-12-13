using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("DbTestFixture")]
    public class GetResultsFromSeasonDbTestFixture : HandlersTestsBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest, IEnumerable<SeasonEventResultModel>>
    {
        public GetResultsFromSeasonDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetResultsFromSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultsFromSeasonRequest> validator)
        {
            return new GetResultsFromSeasonHandler(logger, dbContext, new IValidator<GetResultsFromSeasonRequest>[] { validator });
        }

        protected override GetResultsFromSeasonRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private GetResultsFromSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            return new GetResultsFromSeasonRequest(leagueId, seasonId);
        }

        protected override void DefaultAssertions(GetResultsFromSeasonRequest request, IEnumerable<SeasonEventResultModel> result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var seasonResults = dbContext.ScoredEventResults
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.Event.Schedule.SeasonId == request.SeasonId);
            Assert.Equal(seasonResults.Count(), result.Count());   
        }

        [Fact]
        public async override Task<IEnumerable<SeasonEventResultModel>> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public async override Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }

        [Theory]
        [InlineData(0, testSeasonId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testSeasonId)]
        [InlineData(testLeagueId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long seasonId)
        {
            var request = DefaultRequest(leagueId, seasonId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
