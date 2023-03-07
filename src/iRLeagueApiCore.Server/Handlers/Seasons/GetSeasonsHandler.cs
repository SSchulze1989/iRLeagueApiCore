using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Seasons;

public record GetSeasonsRequest(long LeagueId) : IRequest<IEnumerable<SeasonModel>>;

public sealed class GetSeasonsHandler : SeasonHandlerBase<GetSeasonsHandler, GetSeasonsRequest>, IRequestHandler<GetSeasonsRequest, IEnumerable<SeasonModel>>
{
    public GetSeasonsHandler(ILogger<GetSeasonsHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSeasonsRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<SeasonModel>> Handle(GetSeasonsRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getSeasons = await GetSeasonsEntityAsync(request.LeagueId, cancellationToken);
        if (getSeasons.Count() == 0)
        {
            throw new ResourceNotFoundException();
        }
        return getSeasons;
    }

    private async Task<IEnumerable<SeasonModel>> GetSeasonsEntityAsync(long leagueId, CancellationToken cancellationToken)
    {
        return (await dbContext.Seasons
            .Where(x => x.LeagueId == leagueId)
            .Select(MapToGetSeasonModelExpression)
            .ToListAsync(cancellationToken))
            .OrderBy(x => x.SeasonStart);
    }
}
