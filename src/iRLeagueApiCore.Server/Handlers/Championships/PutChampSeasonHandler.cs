using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record PutChampSeasonRequest(long LeagueId, long ChampSeasonId, PutChampSeasonModel Model) : IRequest<ChampSeasonModel>;

public sealed class PutChampSeasonHandler : ChampSeasonHandlerBase<PutChampSeasonHandler, PutChampSeasonRequest>,
    IRequestHandler<PutChampSeasonRequest, ChampSeasonModel>
{
    public PutChampSeasonHandler(ILogger<PutChampSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutChampSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampSeasonModel> Handle(PutChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putChampSeason = await GetChampSeasonEntityAsync(request.LeagueId, request.ChampSeasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        putChampSeason = await MapToChampSeasonEntityAsync(request.LeagueId, request.Model, putChampSeason, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getChampSeason = await MapToChampSeasonModel(request.LeagueId, putChampSeason.ChampSeasonId, cancellationToken)
            ?? throw new InvalidOperationException("Updated resource not found");
        return getChampSeason;
    }
}
