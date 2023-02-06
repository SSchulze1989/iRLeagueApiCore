using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record PostChampSeasonRequest(long LeagueId, long ChampionshipId, long SeasonId, PostChampSeasonModel Model) : IRequest<ChampSeasonModel>;

public sealed class PostChampSeasonHandler : ChampSeasonHandlerBase<PostChampSeasonHandler, PostChampSeasonRequest>, 
    IRequestHandler<PostChampSeasonRequest, ChampSeasonModel>
{
    public PostChampSeasonHandler(ILogger<PostChampSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostChampSeasonRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampSeasonModel> Handle(PostChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postChampSeason = await CreateChampSeasonEntityAsync(request.LeagueId, request.ChampionshipId, request.SeasonId, cancellationToken);
        postChampSeason = await MapToChampSeasonEntityAsync(request.LeagueId, request.Model, postChampSeason, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getChampSeason = await MapToChampSeasonModel(request.LeagueId, postChampSeason.ChampSeasonId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource not found");
        return getChampSeason;
    }

    private async Task<ChampSeasonEntity> CreateChampSeasonEntityAsync(long leagueId, long championshipId, long seasonId, CancellationToken cancellationToken)
    {
        var championship = await dbContext.Championships
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampionshipId == championshipId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var season = await dbContext.Seasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SeasonId == seasonId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var champSeason = new ChampSeasonEntity()
        {
            Championship = championship,
            Season = season,
        };
        championship.ChampSeasons.Add(champSeason);
        season.ChampSeasons.Add(champSeason);
        dbContext.ChampSeasons.Add(champSeason);
        return champSeason;
    }
}
