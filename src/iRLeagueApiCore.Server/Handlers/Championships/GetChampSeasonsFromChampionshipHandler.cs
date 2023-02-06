using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampSeasonsFromChampionshipRequest(long LeagueId, long ChampionshipId) : IRequest<IEnumerable<ChampSeasonModel>>;

public sealed class GetChampSeasonFromChampionshipHandler : ChampSeasonHandlerBase<GetChampSeasonFromChampionshipHandler, GetChampSeasonsFromChampionshipRequest>,
    IRequestHandler<GetChampSeasonsFromChampionshipRequest, IEnumerable<ChampSeasonModel>>
{
    public GetChampSeasonFromChampionshipHandler(ILogger<GetChampSeasonFromChampionshipHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetChampSeasonsFromChampionshipRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ChampSeasonModel>> Handle(GetChampSeasonsFromChampionshipRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampSeasons = await MapToChampSeasonModelsFromChampionshipAsync(request.LeagueId, request.ChampionshipId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampSeasons;
    }

    private async Task<IEnumerable<ChampSeasonModel>> MapToChampSeasonModelsFromChampionshipAsync(long leagueId, long championshipId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampionshipId == championshipId)
            .OrderByDescending(x => x.Season.SeasonStart)
            .Select(MapToChampSeasonModelExpression)
            .ToListAsync(cancellationToken);
    }
}
