using FluentAssertions;
using FluentValidation;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings
{
    [Collection("HandlerTests")]
    public class PostPointRuleHandlerTests : HandlersTestsBase<PostPointRuleHandler, PostPointRuleRequest, PointRuleModel>
    {
        public PostPointRuleHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostPointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostPointRuleRequest> validator)
        {
            return new PostPointRuleHandler(logger, dbContext, new IValidator<PostPointRuleRequest>[] { validator });
        }

        protected virtual PostPointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
        {
            var PostPointRule = new PostPointRuleModel()
            {
                Name = "TestPointRule",
                BonusPoints = new Dictionary<string, int>() { { "p1", 1 }, { "q1", 1 } },
                FinalSortOptions = new SortOptions[] { SortOptions.PenPtsAsc, SortOptions.TotalPtsAsc },
                MaxPoints = 10,
                PointDropOff = 1,
                PointsPerPlace = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                PointsSortOptions = new SortOptions[] { SortOptions.IntvlAsc, SortOptions.TotalPtsAsc },
            };
            return new PostPointRuleRequest(leagueId, DefaultUser(), PostPointRule);
        }

        protected override PostPointRuleRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testPointRuleId);
        }

        protected override void DefaultAssertions(PostPointRuleRequest request, PointRuleModel result, LeagueDbContext dbContext)
        {
            var testPointRule = dbContext.PointRules
                .SingleOrDefault(x => x.PointRuleId == result.PointRuleId);
            result.PointRuleId.Should().NotBe(0);
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

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }
    }
}
