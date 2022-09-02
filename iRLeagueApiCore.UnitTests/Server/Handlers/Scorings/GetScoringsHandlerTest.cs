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
    public class GetScoringHandlerTest : HandlersTestsBase<GetScoringHandler, GetScoringRequest, ScoringModel>
    {
        public GetScoringHandlerTest(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetScoringRequest> validator)
        {
            return new GetScoringHandler(logger, dbContext, new IValidator<GetScoringRequest>[] { validator });
        }

        protected override GetScoringRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private GetScoringRequest DefaultRequest(long leagueId = testLeagueId, long scoringId = testScoringId)
        {
            return new GetScoringRequest(leagueId, scoringId);
        }

        protected override void DefaultAssertions(GetScoringRequest request, ScoringModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<ScoringModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Theory]
        [InlineData(testLeagueId, 42)]
        [InlineData(2, testScoringId)]
        [InlineData(42, testScoringId)]
        public async Task HandleNotFound(long leagueId, long scoringId)
        {
            var request = DefaultRequest(leagueId, scoringId);
            await HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }
    }
}
