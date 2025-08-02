using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

public sealed class DeleteResultConfigHandlerTests : ResultHandlersTestsBase<DeletePointSystemHandler, DeletePointSystemRequest, MediatR.Unit>
{
    public DeleteResultConfigHandlerTests() : base()
    {
    }

    protected override DeletePointSystemHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeletePointSystemRequest> validator)
    {
        return new DeletePointSystemHandler(logger, dbContext, new IValidator<DeletePointSystemRequest>[] { validator });
    }

    private DeletePointSystemRequest DefaultRequest(long resultConfigId)
    {
        return new DeletePointSystemRequest(resultConfigId);
    }

    protected override DeletePointSystemRequest DefaultRequest()
    {
        return DefaultRequest(TestResultConfigId);
    }

    protected override void DefaultAssertions(DeletePointSystemRequest request, MediatR.Unit result, LeagueDbContext dbContext)
    {
        var deletedResultConfig = dbContext.ResultConfigurations
            .Where(x => x.PointSystemId == request.ResultConfigId)
            .FirstOrDefault();
        deletedResultConfig.Should().BeNull();
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
    public async Task HandleNotFoundAsync(long? leagueId, long? resultId)
    {
        leagueId ??= TestLeagueId;
        resultId ??= TestResultId;
        accessMockHelper.SetCurrentLeague(leagueId.Value);
        var request = DefaultRequest(resultId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
