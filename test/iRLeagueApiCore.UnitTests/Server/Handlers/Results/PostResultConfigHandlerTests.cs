using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("DbTestFixture")]
    public class PostResultConfigDbTestFixture : HandlersTestsBase<PostResultConfigHandler, PostResultConfigRequest, ResultConfigModel>
    {
        public PostResultConfigDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PostResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostResultConfigRequest> validator)
        {
            return new PostResultConfigHandler(logger, dbContext, new IValidator<PostResultConfigRequest>[] { validator });
        }

        protected override PostResultConfigRequest DefaultRequest()
        {
            var postResultConfig = new PostResultConfigModel()
            {
                Name = "TestresultConfig",
                DisplayName = "TestResultConfig DisplayName",
            };
            return new PostResultConfigRequest(testLeagueId, DefaultUser(), postResultConfig);
        }

        protected override void DefaultAssertions(PostResultConfigRequest request, ResultConfigModel result, LeagueDbContext dbContext)
        {
            var expected = request.Model;
            result.LeagueId.Should().Be(request.LeagueId);
            result.ResultConfigId.Should().NotBe(0);
            result.Name.Should().Be(expected.Name);
            result.DisplayName.Should().Be(expected.DisplayName);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<ResultConfigModel> ShouldHandleDefault()
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
