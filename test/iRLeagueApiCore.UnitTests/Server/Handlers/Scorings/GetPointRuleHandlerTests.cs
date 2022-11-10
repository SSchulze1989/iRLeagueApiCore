using FluentAssertions;
using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings
{
    [Collection("DbTestFixture")]
    public class GetPointRuleDbTestFixture : HandlersTestsBase<GetPointRuleHandler, GetPointRuleRequest, PointRuleModel>
    {
        public GetPointRuleDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetPointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetPointRuleRequest> validator)
        {
            return new GetPointRuleHandler(logger, dbContext, new IValidator<GetPointRuleRequest>[] { validator });
        }

        protected virtual GetPointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
        {
            return new GetPointRuleRequest(leagueId, pointRuleId);
        }

        protected override GetPointRuleRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testPointRuleId);
        }

        protected override void DefaultAssertions(GetPointRuleRequest request, PointRuleModel result, LeagueDbContext dbContext)
        {
            var testPointRule = dbContext.PointRules
                .SingleOrDefault(x => x.PointRuleId == request.PointRuleId);
            result.PointRuleId.Should().Be(request.PointRuleId);
            result.BonusPoints.Should().BeEquivalentTo(testPointRule.BonusPoints);
            result.FinalSortOptions.Should().BeEquivalentTo(testPointRule.FinalSortOptions);
            result.LeagueId.Should().Be(request.LeagueId);
            result.MaxPoints.Should().Be(testPointRule.MaxPoints);
            result.Name.Should().Be(testPointRule.Name);
            result.PointDropOff.Should().Be(testPointRule.PointDropOff);
            result.PointsPerPlace.Should().BeEquivalentTo(testPointRule.PointsPerPlace);
            result.PointsSortOptions.Should().BeEquivalentTo(testPointRule.PointsSortOptions);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<PointRuleModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Theory]
        [InlineData(0, testPointRuleId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testPointRuleId)]
        [InlineData(testLeagueId, 42)]
        public async Task ShouldHandleNotFound(long leagueId, long pointRuleId)
        {
            var request = DefaultRequest(leagueId, pointRuleId);
            await HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }
    }
}
