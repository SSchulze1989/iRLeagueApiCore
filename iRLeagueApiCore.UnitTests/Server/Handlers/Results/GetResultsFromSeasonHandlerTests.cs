using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public class GetResultsFromSeasonHandlerTests : HandlersTestsBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest, IEnumerable<GetResultModel>>
    {
        public GetResultsFromSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
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

        protected override void DefaultAssertions(GetResultsFromSeasonRequest request, IEnumerable<GetResultModel> result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var seasonResults = dbContext.ScoredResults
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.Result.Session.Schedule.SeasonId == request.SeasonId);
            Assert.Equal(seasonResults.Count(), result.Count());   
        }

        [Fact]
        public async override Task<IEnumerable<GetResultModel>> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public async override Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
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
