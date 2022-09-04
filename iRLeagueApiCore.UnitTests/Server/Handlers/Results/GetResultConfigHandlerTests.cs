﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Test;
using iRLeagueApiCore.Server.Exceptions;
using System.Linq;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("HandlerTests")]
    public class GetResultConfigHandlerTests : HandlersTestsBase<GetResultConfigHandler, GetResultConfigRequest, ResultConfigModel>
    {
        public GetResultConfigHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultConfigRequest> validator = null)
        {
            return new GetResultConfigHandler(logger, dbContext,
                new IValidator<GetResultConfigRequest>[] { validator ?? MockHelpers.TestValidator<GetResultConfigRequest>() });
        }

        protected virtual GetResultConfigRequest DefaultRequest(long leagueId = testLeagueId, long resultConfigId = testResultConfigId)
        {
            return new GetResultConfigRequest(leagueId, resultConfigId);
        }

        protected override GetResultConfigRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testResultConfigId);
        }

        protected override void DefaultAssertions(GetResultConfigRequest request, ResultConfigModel result, LeagueDbContext dbContext)
        {
            var resultConfigEntity = dbContext.ResultConfigurations
                .FirstOrDefault(x => x.ResultConfigId == request.ResultConfigId);
            resultConfigEntity.Should().NotBeNull();
            result.LeagueId.Should().Be(request.LeagueId);
            result.ResultConfigId.Should().Be(request.ResultConfigId);
            result.Name.Should().Be(resultConfigEntity.Name);
            result.DisplayName.Should().Be(resultConfigEntity.DisplayName);
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
            var request = DefaultRequest(leagueId, resultConfigId);
            await HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }
    }
}
