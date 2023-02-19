using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

[Collection("DbTestFixture")]
public sealed class GetResultConfigDbTestFixture : HandlersTestsBase<GetResultConfigHandler, GetResultConfigRequest, ResultConfigModel>
{
    public GetResultConfigDbTestFixture() : base()
    {
    }



    protected override GetResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultConfigRequest>? validator = null)
    {
        return new GetResultConfigHandler(logger, dbContext,
            new IValidator<GetResultConfigRequest>[] { validator ?? MockHelpers.TestValidator<GetResultConfigRequest>() });
    }

    private GetResultConfigRequest DefaultRequest(long leagueId, long resultConfigId)
    {
        return new GetResultConfigRequest(leagueId, resultConfigId);
    }

    protected override GetResultConfigRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestResultConfigId);
    }

    protected override void DefaultAssertions(GetResultConfigRequest request, ResultConfigModel result, LeagueDbContext dbContext)
    {
        var resultConfigEntity = dbContext.ResultConfigurations
            .FirstOrDefault(x => x.ResultConfigId == request.ResultConfigId);
        resultConfigEntity.Should().NotBeNull();
        result.LeagueId.Should().Be(request.LeagueId);
        result.ResultConfigId.Should().Be(request.ResultConfigId);
        result.Name.Should().Be(resultConfigEntity!.Name);
        result.DisplayName.Should().Be(resultConfigEntity.DisplayName);
        result.ResultsPerTeam.Should().Be(resultConfigEntity.ResultsPerTeam);
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task<ResultConfigModel> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 0)]
    [InlineData(42, defaultId)]
    [InlineData(defaultId, 42)]
    public async Task ShouldHandleNotFoundAsync(long? leagueId, long? resultConfigId)
    {
        leagueId ??= TestLeagueId;
        resultConfigId ??= TestResultConfigId;
        var request = DefaultRequest(leagueId!.Value, resultConfigId!.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
