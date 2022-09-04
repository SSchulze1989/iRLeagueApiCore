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
    public class PutScoringHandlerTests : HandlersTestsBase<PutScoringHandler, PutScoringRequest, ScoringModel>
    {
        private const string NewScoringName = "New scoring Name";

        public PutScoringHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PutScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutScoringRequest> validator)
        {
            return new PutScoringHandler(logger, dbContext, new IValidator<PutScoringRequest>[] { validator });
        }

        protected PutScoringRequest DefaultRequest(long leagueId, long scoringId)
        {
            var model = new PutScoringModel()
            {
                Name = NewScoringName,
            };
            return new PutScoringRequest(leagueId, scoringId, model);
        }

        protected override PutScoringRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScoringId);
        }

        protected override void DefaultAssertions(PutScoringRequest request, ScoringModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            Assert.Equal(NewScoringName, result.Name);
        }

        [Fact]
        public override async Task<ScoringModel> ShouldHandleDefaultAsync()
        {
            return await base.ShouldHandleDefaultAsync();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailedAsync()
        {
            await base.ShouldHandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(testLeagueId, 0)]
        [InlineData(0, testScoringId)]
        [InlineData(testLeagueId, 42)]
        [InlineData(42, testScoringId)]
        public async Task HandleNotFoundAsync(long leagueId, long scoringId)
        {
            var request = DefaultRequest(leagueId, scoringId);
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await HandleSpecialAsync(request, null));
        }
    }
}
