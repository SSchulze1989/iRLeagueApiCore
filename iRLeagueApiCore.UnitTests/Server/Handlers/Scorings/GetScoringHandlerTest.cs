using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings
{
    [Collection("HandlerTests")]
    public class GetScoringsHandlerTest : HandlersTestsBase<GetScoringsHandler, GetScoringsRequest, IEnumerable<ScoringModel>>
    {
        public GetScoringsHandlerTest(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetScoringsHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScoringsRequest> validator)
        {
            return new GetScoringsHandler(logger, dbContext, new IValidator<GetScoringsRequest>[] { validator });
        }

        protected override GetScoringsRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private GetScoringsRequest DefaultRequest(long leagueId = testLeagueId, long scoringId = 0)
        {
            return new GetScoringsRequest(leagueId);
        }

        protected override void DefaultAssertions(GetScoringsRequest request, IEnumerable<ScoringModel> result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            Assert.NotEmpty(result);
        }

        [Fact]
        public override async Task<IEnumerable<ScoringModel>> ShouldHandleDefaultAsync()
        {
            return await base.ShouldHandleDefaultAsync();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailedAsync()
        {
            await base.ShouldHandleValidationFailedAsync();
        }
    }
}
