﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

public sealed class GetResultConfigHandlerTests : ResultHandlersTestsBase<GetResultConfigHandler, GetResultConfigRequest, ResultConfigModel>
{
    public GetResultConfigHandlerTests() : base()
    {
    }

    protected override GetResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultConfigRequest>? validator = null)
    {
        return new GetResultConfigHandler(logger, dbContext,
            new IValidator<GetResultConfigRequest>[] { validator ?? MockHelpers.TestValidator<GetResultConfigRequest>() });
    }

    private GetResultConfigRequest DefaultRequest(long resultConfigId)
    {
        return new GetResultConfigRequest(resultConfigId);
    }

    protected override GetResultConfigRequest DefaultRequest()
    {
        return DefaultRequest(TestResultConfigId);
    }

    protected override void DefaultAssertions(GetResultConfigRequest request, ResultConfigModel result, LeagueDbContext dbContext)
    {
        var resultConfigEntity = dbContext.ResultConfigurations
            .FirstOrDefault(x => x.ResultConfigId == request.ResultConfigId);
        resultConfigEntity.Should().NotBeNull();
        result.ResultConfigId.Should().Be(request.ResultConfigId);
        result.Name.Should().Be(resultConfigEntity!.Name);
        result.DisplayName.Should().Be(resultConfigEntity.DisplayName);
        result.ResultsPerTeam.Should().Be(resultConfigEntity.ResultsPerTeam);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task ShouldHandleDefault()
    {
        await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0L, defaultId)]
    [InlineData(defaultId, 0L)]
    [InlineData(-42L, defaultId)]
    [InlineData(defaultId, -42L)]
    public async Task ShouldHandleNotFoundAsync(long? leagueId, long? resultConfigId)
    {
        leagueId ??= TestLeagueId;
        resultConfigId ??= TestResultConfigId;
        accessMockHelper.SetCurrentLeague(leagueId.Value);
        var request = DefaultRequest(resultConfigId!.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
