using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public class PostSeasonHandlerTests : HandlersTestsBase<PostSeasonHandler, PostSeasonRequest, SeasonModel>
    {
        private const string testSeasonName = "TestSeason";

        public PostSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostSeasonRequest> validator)
        {
            return new PostSeasonHandler(logger, dbContext, new IValidator<PostSeasonRequest>[] { validator });
        }

        protected override PostSeasonRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private PostSeasonRequest DefaultRequest(long leagueId = testLeagueId)
        {
            var model = new PostSeasonModel()
            {
                SeasonName = testSeasonName
            };
            return new PostSeasonRequest(leagueId, DefaultUser(), model);
        }

        protected override void DefaultAssertions(PostSeasonRequest request, SeasonModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            Assert.Contains(dbContext.Seasons, x => x.SeasonId == result.SeasonId);
            Assert.Equal(request.Model.SeasonName, dbContext.Seasons.Single(x => x.SeasonId == result.SeasonId).SeasonName);
            Assert.Equal(request.Model.SeasonName, result.SeasonName);
        }

        [Fact]
        public async override Task<SeasonModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task HandleNotFound(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
