using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using System.Diagnostics.Contracts;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;
public sealed class GetResultConfigsFromSeasonHandlerTests : ResultHandlersTestsBase<GetResultConfigsFromSeasonHandler, GetResultConfigsFromSeasonRequest, IEnumerable<ResultConfigModel>>
{
    protected override GetResultConfigsFromSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultConfigsFromSeasonRequest> validator)
    {
        return new(logger, dbContext, new[] { validator });
    }

    protected override GetResultConfigsFromSeasonRequest DefaultRequest() => DefaultRequest(TestLeagueId, TestSeasonId);

    private GetResultConfigsFromSeasonRequest DefaultRequest(long leagueId, long seasonId) => new(leagueId, seasonId);

    protected override void DefaultAssertions(GetResultConfigsFromSeasonRequest request, IEnumerable<ResultConfigModel> result, LeagueDbContext dbContext)
    {
        base.DefaultAssertions(request, result, dbContext);
        var champSeasons = dbContext.ChampSeasons
            .Where(x => x.SeasonId == request.SeasonId);
        var resultConfig = champSeasons.SelectMany(x => x.ResultConfigurations);
        result.Should().HaveSameCount(resultConfig);
    }

    [Fact]
    public override Task<IEnumerable<ResultConfigModel>> ShouldHandleDefault()
    {
        return base.ShouldHandleDefault();
    }
}
