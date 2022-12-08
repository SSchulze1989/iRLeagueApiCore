using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings
{
    [Collection("DbTestFixture")]
    public class DeleteScoringDbTestFixture : HandlersTestsBase<DeleteScoringHandler, DeleteScoringRequest, Unit>
    {
        public DeleteScoringDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeleteScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteScoringRequest> validator)
        {
            return new DeleteScoringHandler(logger, dbContext, new IValidator<DeleteScoringRequest>[] { validator });
        }

        protected override DeleteScoringRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        protected DeleteScoringRequest DefaultRequest(long leagueId = testLeagueId, long scoringId = testScoringId)
        {
            return new DeleteScoringRequest(leagueId, scoringId);
        }

        protected override void DefaultAssertions(DeleteScoringRequest request, Unit result, LeagueDbContext dbContext)
        {
            Assert.DoesNotContain(dbContext.Scorings, x => x.ScoringId == request.ScoringId);
        }

        [Fact]
        public override async Task<Unit> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Theory]
        [InlineData(testLeagueId, 0)]
        [InlineData(0, testScoringId)]
        [InlineData(testLeagueId, 42)]
        [InlineData(42, testScoringId)]
        public async Task HandleNotFoundAsync(long leagueId, long scoringId)
        {
            var request = DefaultRequest(leagueId, scoringId);
            await HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }
    }
}
