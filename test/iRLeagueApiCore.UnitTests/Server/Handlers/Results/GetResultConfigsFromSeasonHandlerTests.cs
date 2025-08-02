using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;
public sealed class GetResultConfigsFromSeasonHandlerTests : ResultHandlersTestsBase<GetPointSystemsFromSeasonHandler, GetPointSystemsFromSeasonRequest, IEnumerable<PointSystemModel>>
{
    protected override GetPointSystemsFromSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetPointSystemsFromSeasonRequest> validator)
    {
        return new(logger, dbContext, new[] { validator });
    }

    protected override GetPointSystemsFromSeasonRequest DefaultRequest() => DefaultRequest(TestSeasonId);

    private GetPointSystemsFromSeasonRequest DefaultRequest(long seasonId) => new(seasonId);

    protected override void DefaultAssertions(GetPointSystemsFromSeasonRequest request, IEnumerable<PointSystemModel> result, LeagueDbContext dbContext)
    {
        base.DefaultAssertions(request, result, dbContext);
        var champSeasons = dbContext.ChampSeasons
            .Where(x => x.IsActive)
            .Where(x => x.SeasonId == request.SeasonId);
        var resultConfig = champSeasons.SelectMany(x => x.PointSystems);
        result.Should().HaveSameCount(resultConfig);
    }

    [Fact]
    public async override Task ShouldHandleDefault()
    {
        await base.ShouldHandleDefault();
    }

    [Fact]
    public async Task ShouldHandle_WhenChampSeasonIsInactive()
    {
        var inActiveChampSeason = await dbContext.ChampSeasons.LastAsync();
        inActiveChampSeason.IsActive = false;
        await dbContext.SaveChangesAsync();
        await base.ShouldHandleDefault();
    }
}
