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
    public class PutLeagueHandlerTests : HandlersTestsBase<PutLeagueHandler, PutLeagueRequest, GetLeagueModel>
    {
        public PutLeagueHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PutLeagueHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutLeagueRequest> validator)
        {
            return new PutLeagueHandler(logger, dbContext, new IValidator<PutLeagueRequest>[] { validator });
        }

        protected override PutLeagueRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        public PutLeagueRequest DefaultRequest(long leagueId = testLeagueId)
        {
            var model = new PutLeagueModel()
            {
                NameFull = "Put league test"
            };
            return new PutLeagueRequest(leagueId, DefaultUser(), model);
        }

        [Fact]
        public override async Task<GetLeagueModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task HandleNotFoundAsync(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await base.HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public async override Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }
    }
}
