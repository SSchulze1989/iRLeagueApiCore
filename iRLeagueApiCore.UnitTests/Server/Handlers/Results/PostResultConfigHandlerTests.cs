using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("HandlerTests")]
    public class PostResultConfigHandlerTests : HandlersTestsBase<PostResultConfigHandler, PostResultConfigRequest, ResultConfigModel>
    {
        public PostResultConfigHandlerTests(DbTestFixture fixture) : base(fixture)
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
            result.LeagueId.Should().Be(request.LeagueId);
            result.ResultConfigId.Should().Be(result.ResultConfigId);
            result.Name.Should().Be(request.Model.Name);
            result.DisplayName.Should().Be(request.Model.DisplayName);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<ResultConfigModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }
    }
}
