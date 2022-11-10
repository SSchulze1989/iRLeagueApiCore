using FluentAssertions;
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
    [Collection("DbTestFixture")]
    public class PutScoringDbTestFixture : HandlersTestsBase<PutScoringHandler, PutScoringRequest, ScoringModel>
    {
        private const string NewScoringName = "New scoring Name";

        public PutScoringDbTestFixture(DbTestFixture fixture) : base(fixture)
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
            return new PutScoringRequest(leagueId, scoringId, DefaultUser(), model);
        }

        protected override PutScoringRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScoringId);
        }

        protected override void DefaultAssertions(PutScoringRequest request, ScoringModel result, LeagueDbContext dbContext)
        {
            var expected = request.Model;
            result.LeagueId.Should().Be(request.LeagueId);
            result.Id.Should().Be(request.ScoringId);
            result.Name.Should().Be(expected.Name);
            result.ScoringKind.Should().Be(expected.ScoringKind);
            result.ExtScoringSourceId.Should().Be(expected.ExtScoringSourceId);
            result.MaxResultsPerGroup.Should().Be(expected.MaxResultsPerGroup);
            result.ShowResults.Should().Be(expected.ShowResults);
            result.UpdateTeamOnRecalculation.Should().Be(expected.UpdateTeamOnRecalculation);
            result.UseResultSetTeam.Should().Be(expected.UseResultSetTeam);
            AssertChanged(request.User, DateTime.UtcNow, result);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<ScoringModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
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
