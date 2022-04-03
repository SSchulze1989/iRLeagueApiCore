using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues
{
    public class PostLeagueHandlerTests : HandlersTestsBase<PostLeagueHandler, PostLeagueRequest, GetLeagueModel>
    {
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
                Name = testLeagueName,
                NameFull = "Full test league name"
            };
            return CreateRequest(DefaultUser(), model);
        }

        protected PostLeagueRequest CreateRequest(LeagueUser user, PostLeagueModel model)
        {
            return new PostLeagueRequest(user, model);
        }

    }
}
