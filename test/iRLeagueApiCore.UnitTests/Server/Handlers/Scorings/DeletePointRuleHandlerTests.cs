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
    [Collection("HandlerTests")]
    public class DeletePointRuleHandlerTests : HandlersTestsBase<DeletePointRuleHandler, DeletePointRuleRequest, MediatR.Unit>
    {
        public DeletePointRuleHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeletePointRuleHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeletePointRuleRequest> validator)
        {
            return new DeletePointRuleHandler(logger, dbContext, new IValidator<DeletePointRuleRequest>[] { validator });
        }

        protected virtual DeletePointRuleRequest DefaultRequest(long leagueId, long pointRuleId)
        {
            return new DeletePointRuleRequest(leagueId, pointRuleId);
        }

        protected override DeletePointRuleRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testPointRuleId);
        }

        protected override void DefaultAssertions(DeletePointRuleRequest request, MediatR.Unit result, LeagueDbContext dbContext)
        {
            var testPointRule = dbContext.PointRules
                .SingleOrDefault(x => x.PointRuleId == request.PointRuleId);
            testPointRule.Should().BeNull();
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<MediatR.Unit> ShouldHandleDefault()
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
