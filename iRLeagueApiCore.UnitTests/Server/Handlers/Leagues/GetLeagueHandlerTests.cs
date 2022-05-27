using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues
{
    [Collection("HandlerTests")]
    public class GetLeagueHandlerTests : HandlersTestsBase<GetLeagueHandler, GetLeagueRequest, LeagueModel>
    {
        public GetLeagueHandlerTests(DbTestFixture fixture) : base(fixture)
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
        public override async Task<LeagueModel> HandleDefaultAsync()
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
        public async Task HandleNotFoundAsync(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
