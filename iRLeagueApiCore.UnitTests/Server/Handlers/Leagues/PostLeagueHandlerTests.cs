using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;
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
    public class PostLeagueHandlerTests : HandlersTestsBase<PostLeagueHandler, PostLeagueRequest, LeagueModel>
    {
        private const string postLeagueName = "PostLeague";

        public PostLeagueHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostLeagueHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostLeagueRequest> validator)
        {
            return new PostLeagueHandler(logger, dbContext, new IValidator<PostLeagueRequest>[] { validator });
        }

        protected override PostLeagueRequest DefaultRequest()
        {
            var model = new PostLeagueModel()
            {
                Name = postLeagueName,
                NameFull = "Full test league name"
            };
            return CreateRequest(DefaultUser(), model);
        }

        protected PostLeagueRequest CreateRequest(LeagueUser user, PostLeagueModel model)
        {
            return new PostLeagueRequest(user, model);
        }

        [Fact]
        public override async Task<LeagueModel> HandleDefaultAsync()
        {
            var result = await base.HandleDefaultAsync();
            return result;
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }
    }
}