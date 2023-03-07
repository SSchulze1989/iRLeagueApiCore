using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampSeasonRequest(long LeagueId, long ChampSeasonId) : IRequest<ChampSeasonModel>;

public sealed class GetChampSeasonHandler : ChampSeasonHandlerBase<GetChampSeasonHandler, GetChampSeasonRequest>,
    IRequestHandler<GetChampSeasonRequest, ChampSeasonModel>
{
    public GetChampSeasonHandler(ILogger<GetChampSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetChampSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampSeasonModel> Handle(GetChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampSeason = await MapToChampSeasonModel(request.LeagueId, request.ChampSeasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampSeason;
    }
}