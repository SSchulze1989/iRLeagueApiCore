using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Seasons
{
    [Collection("HandlerTests")]
    public class PutSeasonHandlerTests : HandlersTestsBase<PutSeasonHandler, PutSeasonRequest, SeasonModel>
    {
        private const string testSeasonName = "TestSeason";

        public PutSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PutSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutSeasonRequest> validator)
        {
            return new PutSeasonHandler(logger, dbContext, new IValidator<PutSeasonRequest>[] { validator });
        }

        protected override PutSeasonRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private PutSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            var model = new PutSeasonModel()
            {
                SeasonName = testSeasonName
            };
            return new PutSeasonRequest(leagueId, DefaultUser(), seasonId, model);
        }

        protected override void DefaultPreTestAssertions(PutSeasonRequest request, LeagueDbContext dbContext)
        {
            Assert.NotEqual(dbContext.Seasons.Single(x => x.SeasonId == request.SeasonId).SeasonName, request.Model.SeasonName);
        }

        protected override void DefaultAssertions(PutSeasonRequest request, SeasonModel result, LeagueDbContext dbContext)
        {
            Assert.Equal(dbContext.Seasons.Single(x => x.SeasonId == request.SeasonId).SeasonName, request.Model.SeasonName);
            Assert.Equal(request.Model.SeasonName, result.SeasonName);
            Assert.Equal(request.SeasonId, result.SeasonId);
        }

        [Fact]
        public async override Task<SeasonModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 42)]
        [InlineData(42, 1)]
        public async Task HandleNotFound(long leagueId, long seasonId)
        {
            var request = DefaultRequest(leagueId, seasonId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
