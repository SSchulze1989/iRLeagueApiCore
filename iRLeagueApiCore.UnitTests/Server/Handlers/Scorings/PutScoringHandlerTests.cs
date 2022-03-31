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
    public class PutScoringHandlerTests : HandlersTestsBase<PutScoringHandler, PutScoringRequest, GetScoringModel>
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
                BasePoints = new double[0],
                BonusPoints = new string[0]
            };
            return new PutScoringRequest(leagueId, scoringId, model);
        }

        protected override PutScoringRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScoringId);
        }

        protected override void DefaultAssertions(PutScoringRequest request, GetScoringModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            Assert.Equal(NewScoringName, result.Name);
            Assert.Empty(result.BasePoints);
            Assert.Empty(result.BonusPoints);
        }

        [Fact]
        public override async Task<GetScoringModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
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
