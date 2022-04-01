using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public class GetScoringsHandlerTest : HandlersTestsBase<GetScoringsHandler, GetScoringsRequest, IEnumerable<GetScoringModel>>
    {
        public GetScoringsHandlerTest(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetScoringsHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScoringsRequest> validator)
        {
            return new GetScoringsHandler(dbContext, new IValidator<GetScoringsRequest>[] { validator });
        }

        protected override GetScoringsRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private GetScoringsRequest DefaultRequest(long leagueId = testLeagueId, long scoringId = 0)
        {
            return new GetScoringsRequest(leagueId);
        }

        protected override void DefaultAssertions(GetScoringsRequest request, IEnumerable<GetScoringModel> result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            Assert.NotEmpty(result);
            Assert.NotNull(result.First().BasePoints);
            Assert.NotNull(result.First().BonusPoints);
        }

        [Fact]
        public override async Task<IEnumerable<GetScoringModel>> HandleDefaultAsync()
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
