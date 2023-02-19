﻿using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

[Collection("DbTestFixture")]
public sealed class DeleteResultConfigDbTestFixture : HandlersTestsBase<DeleteResultConfigHandler, DeleteResultConfigRequest, MediatR.Unit>
{
    public DeleteResultConfigDbTestFixture() : base()
    {
    }

    protected override DeleteResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteResultConfigRequest> validator)
    {
        return new DeleteResultConfigHandler(logger, dbContext, new IValidator<DeleteResultConfigRequest>[] { validator });
    }

    private DeleteResultConfigRequest DefaultRequest(long leagueId, long resultConfigId)
    {
        return new DeleteResultConfigRequest(leagueId, resultConfigId);
    }

    protected override DeleteResultConfigRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestResultConfigId);
    }

    protected override void DefaultAssertions(DeleteResultConfigRequest request, MediatR.Unit result, LeagueDbContext dbContext)
    {
        var deletedResultConfig = dbContext.ResultConfigurations
            .Where(x => x.ResultConfigId == request.ResultConfigId)
            .FirstOrDefault();
        deletedResultConfig.Should().BeNull();
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<MediatR.Unit> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 0)]
    [InlineData(42, defaultId)]
    [InlineData(defaultId, 42)]
    public async Task HandleNotFoundAsync(long? leagueId, long? resultId)
    {
        leagueId ??= TestLeagueId;
        resultId ??= TestResultId;
        var request = DefaultRequest(leagueId.Value, resultId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
