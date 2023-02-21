using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

[Collection("DbTestFixture")]
public sealed class PutResultConfigHandlerTests : ResultHandlersTestsBase<PutResultConfigHandler, PutResultConfigRequest, ResultConfigModel>
{
    public PutResultConfigHandlerTests() : base()
    {
    }

    protected override PutResultConfigHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutResultConfigRequest>? validator = null)
    {
        return new PutResultConfigHandler(logger, dbContext,
            new IValidator<PutResultConfigRequest>[] { validator ?? MockHelpers.TestValidator<PutResultConfigRequest>() });
    }

    private PutResultConfigRequest DefaultRequest(long leagueId, long resultConfigId)
    {
        var PutResultConfig = new PutResultConfigModel()
        {
            Name = "TestresultConfig",
            DisplayName = "TestResultConfig DisplayName",
            ResultsPerTeam = 10,
        };
        return new PutResultConfigRequest(leagueId, resultConfigId, DefaultUser(), PutResultConfig);
    }

    protected override PutResultConfigRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestResultConfigId);
    }

    protected override void DefaultAssertions(PutResultConfigRequest request, ResultConfigModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.LeagueId.Should().Be(request.LeagueId);
        result.ResultConfigId.Should().Be(request.ResultConfigId);
        result.Name.Should().Be(expected.Name);
        result.DisplayName.Should().Be(expected.DisplayName);
        result.ResultsPerTeam.Should().Be(expected.ResultsPerTeam);
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
        using var dbContext = accessMockHelper.CreateMockDbContext();
        var handler = CreateTestHandler(dbContext);
        var request = DefaultRequest(leagueId.Value, resultConfigId.Value);
        var act = () => handler.Handle(request, default);
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
