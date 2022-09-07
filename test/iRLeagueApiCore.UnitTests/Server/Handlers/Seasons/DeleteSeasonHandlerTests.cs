
using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Seasons
{
    [Collection("HandlerTests")]
    public class DeleteSeasonHandlerTests : HandlersTestsBase<DeleteSeasonHandler, DeleteSeasonRequest, Unit>
    {
        private const string testSeasonName = "TestSeason";

        public DeleteSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeleteSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteSeasonRequest> validator)
        {
            return new DeleteSeasonHandler(logger, dbContext, new IValidator<DeleteSeasonRequest>[] { validator });
        }

        protected override DeleteSeasonRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testSeasonId);
        }

        private DeleteSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {

            return new DeleteSeasonRequest(leagueId, seasonId);
        }

        protected override void DefaultPreTestAssertions(DeleteSeasonRequest request, LeagueDbContext dbContext)
        {
            Assert.Contains(dbContext.Seasons, x => x.SeasonId == request.SeasonId);
        }

        protected override void DefaultAssertions(DeleteSeasonRequest request, Unit result, LeagueDbContext dbContext)
        {
            Assert.DoesNotContain(dbContext.Seasons, x => x.SeasonId == request.SeasonId);
        }

        [Fact]
        public async override Task<Unit> ShouldHandleDefault()
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
        [InlineData(42, 1)]
        [InlineData(1, 42)]
        public async Task HandleNotFound(long leagueId, long seasonId)
        {
            var request = DefaultRequest(leagueId, seasonId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
