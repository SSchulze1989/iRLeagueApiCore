namespace iRLeagueApiCore.Server.Handlers.Seasons;

public record DeleteSeasonRequest(long SeasonId) : IRequest<Unit>;

public sealed class DeleteSeasonHandler : SeasonHandlerBase<DeleteSeasonHandler, DeleteSeasonRequest, Unit>
{
    public DeleteSeasonHandler(ILogger<DeleteSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        await DeleteSeasonEntity(request.SeasonId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task DeleteSeasonEntity(long seasonId, CancellationToken cancellationToken)
    {
        var deleteSeason = await dbContext.Seasons
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.PointSystems)
                    .ThenInclude(x => x.PointFilters)
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.PointSystems)
                    .ThenInclude(x => x.ResultFilters)
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.DefaultPointSystem)
            .SingleOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        foreach (var champSeason in deleteSeason.ChampSeasons)
        {
            var deleteFilterOptions = await dbContext.FilterOptions
                .Where(x => x.ChampSeasonId == champSeason.ChampSeasonId)
                .ToListAsync(cancellationToken);
            dbContext.RemoveRange(deleteFilterOptions);
            dbContext.RemoveRange(champSeason.PointSystems);
            champSeason.DefaultPointSystem = null;
            champSeason.PointSystems = [];
            champSeason.StandingConfiguration = null;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.Seasons.Remove(deleteSeason);
    }
}
