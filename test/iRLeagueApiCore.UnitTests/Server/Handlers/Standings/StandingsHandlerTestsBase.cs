using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Standings;

public abstract class StandingsHandlerTestsBase<THandler, TRequest, TResult> :
    HandlersTestsBase<THandler, TRequest, TResult>
    where THandler : IRequestHandler<TRequest, TResult>
    where TRequest : class, IRequest<TResult>
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await CreateTestStanding(dbContext);
    }

    protected async Task CreateTestStanding(LeagueDbContext db)
    {
        var standing = new StandingEntity
        {
            LeagueId = TestLeagueId,
            SeasonId = TestSeasonId,
            EventId = TestEventId,
            Name = "Test Standing",
            StandingRows = new HashSet<StandingRowEntity>()
        };
        db.Standings.Add(standing);
        await db.SaveChangesAsync();
    }
}
