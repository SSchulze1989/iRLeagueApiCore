using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

public sealed class PutResultConfigHandlerTests : ResultHandlersTestsBase<PutPointSystemHandler, PutPointSystemRequest, PointSystemModel>
{
    public PutResultConfigHandlerTests() : base()
    {
    }

    protected override PutPointSystemHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutPointSystemRequest>? validator = null)
    {
        return new PutPointSystemHandler(logger, dbContext,
            new IValidator<PutPointSystemRequest>[] { validator ?? MockHelpers.TestValidator<PutPointSystemRequest>() });
    }

    private PutPointSystemRequest DefaultRequest(long resultConfigId)
    {
        var PutResultConfig = new PutPointSystemModel()
        {
            Name = "TestresultConfig",
            DisplayName = "TestResultConfig DisplayName",
            ResultsPerTeam = 10,
        };
        return new PutPointSystemRequest(resultConfigId, DefaultUser(), PutResultConfig);
    }

    protected override PutPointSystemRequest DefaultRequest()
    {
        return DefaultRequest(TestResultConfigId);
    }

    protected override void DefaultAssertions(PutPointSystemRequest request, PointSystemModel result, LeagueDbContext dbContext)
    {
        var expected = request.Model;
        result.PointSystemId.Should().Be(request.ResultConfigId);
        result.Name.Should().Be(expected.Name);
        result.DisplayName.Should().Be(expected.DisplayName);
        result.ResultsPerTeam.Should().Be(expected.ResultsPerTeam);
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
        using var dbContext = accessMockHelper.CreateMockDbContext(databaseName);
        var handler = CreateTestHandler(dbContext);
        var request = DefaultRequest(resultConfigId.Value);
        var act = () => handler.Handle(request, default);
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
