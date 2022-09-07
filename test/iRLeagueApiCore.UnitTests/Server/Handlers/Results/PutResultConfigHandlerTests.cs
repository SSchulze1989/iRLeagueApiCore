using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Test;
using iRLeagueApiCore.Server.Exceptions;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("HandlerTests")]
    public class PutResultConfigHandlerTests : HandlersTestsBase<PutResultConfigHandler, PutResultConfigRequest, ResultConfigModel>
    {
        public PutResultConfigHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PutResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutResultConfigRequest> validator = null)
        {
            return new PutResultConfigHandler(logger, dbContext, 
                new IValidator<PutResultConfigRequest>[] { validator ?? MockHelpers.TestValidator<PutResultConfigRequest>() });
        }

        protected virtual PutResultConfigRequest DefaultRequest(long leagueId = testLeagueId, long resultConfigId = testResultConfigId)
        {
            var PutResultConfig = new PutResultConfigModel()
            {
                Name = "TestresultConfig",
                DisplayName = "TestResultConfig DisplayName",
            };
            return new PutResultConfigRequest(leagueId, resultConfigId, DefaultUser(), PutResultConfig);
        }

        protected override PutResultConfigRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testResultConfigId);
        }

        protected override void DefaultAssertions(PutResultConfigRequest request, ResultConfigModel result, LeagueDbContext dbContext)
        {
            result.LeagueId.Should().Be(request.LeagueId);
            result.ResultConfigId.Should().Be(request.ResultConfigId);
            result.Name.Should().Be(request.Model.Name);
            result.DisplayName.Should().Be(request.Model.DisplayName);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<ResultConfigModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Theory]
        [InlineData(0, testResultConfigId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testResultConfigId)]
        [InlineData(testLeagueId, 42)]
        public async Task ShouldHandleNotFoundAsync(long leagueId, long resultConfigId)
        {
            using var dbContext = fixture.CreateDbContext();
            var handler = CreateTestHandler(dbContext);
            var request = DefaultRequest(leagueId, resultConfigId);
            var act = () => handler.Handle(request, default);
            await act.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }
    }
}
