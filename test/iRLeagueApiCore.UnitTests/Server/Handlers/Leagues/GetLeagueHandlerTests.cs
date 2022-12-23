using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues
{
    [Collection("DbTestFixture")]
    public class GetLeagueDbTestFixture : HandlersTestsBase<GetLeagueHandler, GetLeagueRequest, LeagueModel>
    {
        public GetLeagueDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetLeagueHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetLeagueRequest> validator)
        {
            return new GetLeagueHandler(logger, dbContext, new IValidator<GetLeagueRequest>[] { validator });
        }

        protected override GetLeagueRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetLeagueRequest DefaultRequest(long leagueId)
        {
            return new GetLeagueRequest(leagueId);
        }

        [Fact]
        public override async Task<LeagueModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task HandleNotFoundAsync(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
