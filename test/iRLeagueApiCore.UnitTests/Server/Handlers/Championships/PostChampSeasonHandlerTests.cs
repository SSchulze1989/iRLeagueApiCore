using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Championships;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Championships;
public sealed class PostChampSeasonHandlerTests : HandlersTestsBase<PostChampSeasonHandler, PostChampSeasonRequest, ChampSeasonModel>
{
    private const long testChampionshipId = 1;

    public PostChampSeasonHandlerTests(DbTestFixture dbFixture) : base(dbFixture)
    {
    }

    protected override PostChampSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostChampSeasonRequest> validator)
    {
        return new PostChampSeasonHandler(logger, dbContext, new[] { validator });
    }

    protected override PostChampSeasonRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId, testChampionshipId, testSeasonId);
    }

    private PostChampSeasonRequest DefaultRequest(long leagueId, long championshipId, long seasonId)
    {
        return new(leagueId, championshipId, seasonId, DefaultUser(), null);
    }
}
