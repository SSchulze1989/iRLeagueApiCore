using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("HandlerTests")]
    public class DeleteResultConfigHandlerTests : HandlersTestsBase<DeleteResultConfigHandler, DeleteResultConfigRequest, MediatR.Unit>
    {
        public DeleteResultConfigHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeleteResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteResultConfigRequest> validator)
        {
            return new DeleteResultConfigHandler(logger, dbContext, new IValidator<DeleteResultConfigRequest>[] { validator });
        }

        protected virtual DeleteResultConfigRequest DefaultRequest(long leagueId, long resultConfigId)
        {
            return new DeleteResultConfigRequest(leagueId, resultConfigId);
        }

        protected override DeleteResultConfigRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testResultConfigId);
        }

        protected override async void DefaultAssertions(DeleteResultConfigRequest request, MediatR.Unit result, LeagueDbContext dbContext)
        {
            var deletedResultConfig = await dbContext.ResultConfigurations
                .Where(x => x.ResultConfigId == request.ResultConfigId)
                .FirstOrDefaultAsync();
            deletedResultConfig.Should().BeNull();
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<MediatR.Unit> ShouldHandleDefaultAsync()
        {
            return await base.ShouldHandleDefaultAsync();
        }

        [Theory]
        [InlineData(0, testResultId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testResultId)]
        [InlineData(testLeagueId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long resultId)
        {
            var request = DefaultRequest(leagueId, resultId);
            await HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public override async Task ShouldHandleValidationFailedAsync()
        {
            await base.ShouldHandleValidationFailedAsync();
        }
    }
}
