using FluentValidation;
using iRLeagueApiCore.Common.Models;
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
    public class GetLeaguesHandlerTests : HandlersTestsBase<GetLeaguesHandler, GetLeaguesRequest, IEnumerable<LeagueModel>>
    {
        public GetLeaguesHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetLeaguesHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetLeaguesRequest> validator)
        {
            return new GetLeaguesHandler(logger, dbContext, new IValidator<GetLeaguesRequest>[] { validator });
        }

        protected override GetLeaguesRequest DefaultRequest()
        {
            return new GetLeaguesRequest();
        }

        [Fact]
        public override async Task<IEnumerable<LeagueModel>> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }
    }
}
