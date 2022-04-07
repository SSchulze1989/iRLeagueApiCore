using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Sessions
{
    [Collection("HandlerTests")]
    public class GetSessionsHandlerTests : HandlersTestsBase<GetSessionsHandler, GetSessionsRequest, IEnumerable<GetSessionModel>>
    {
        public GetSessionsHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetSessionsHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSessionsRequest> validator)
        {
            return new GetSessionsHandler(logger, dbContext, new IValidator<GetSessionsRequest>[] { validator });
        }

        protected override GetSessionsRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetSessionsRequest DefaultRequest(long leagueId)
        {
            return new GetSessionsRequest(leagueId);
        }

        [Fact]
        public async override Task<IEnumerable<GetSessionModel>> HandleDefaultAsync()
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
