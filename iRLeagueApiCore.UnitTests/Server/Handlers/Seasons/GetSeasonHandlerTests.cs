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
    public class GetSeasonHandlerTests : HandlersTestsBase<GetSeasonHandler, GetSeasonRequest, GetSeasonModel>
    {
        public GetSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSeasonRequest> validator)
        {
            return new GetSeasonHandler(logger, dbContext, new IValidator<GetSeasonRequest>[] { validator });
        }

        protected override GetSeasonRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            return new GetSeasonRequest(leagueId, seasonId);
        }

        [Fact]
        public async override Task<GetSeasonModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
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
