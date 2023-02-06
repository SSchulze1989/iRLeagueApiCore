using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampSeasonsFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<ChampSeasonModel>>;

public sealed class GetChampSeasonFromSeasonHandler : ChampSeasonHandlerBase<GetChampSeasonFromSeasonHandler, GetChampSeasonsFromSeasonRequest>,
    IRequestHandler<GetChampSeasonsFromSeasonRequest, IEnumerable<ChampSeasonModel>>
{
    public GetChampSeasonFromSeasonHandler(ILogger<GetChampSeasonFromSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetChampSeasonsFromSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ChampSeasonModel>> Handle(GetChampSeasonsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampSeasons = await MapToChampSeasonModelsFromSeasonAsync(request.LeagueId, request.SeasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampSeasons;
    }

    private async Task<IEnumerable<ChampSeasonModel>> MapToChampSeasonModelsFromSeasonAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SeasonId == seasonId)
            .Select(MapToChampSeasonModelExpression)
            .ToListAsync(cancellationToken);
    }
}