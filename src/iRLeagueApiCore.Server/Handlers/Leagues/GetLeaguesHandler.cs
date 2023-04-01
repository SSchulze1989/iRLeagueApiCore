using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Leagues;

public record GetLeaguesRequest() : IRequest<IEnumerable<LeagueModel>>;

public sealed class GetLeaguesHandler : LeagueHandlerBase<GetLeaguesHandler, GetLeaguesRequest>, IRequestHandler<GetLeaguesRequest, IEnumerable<LeagueModel>>
{
    public GetLeaguesHandler(ILogger<GetLeaguesHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetLeaguesRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<LeagueModel>> Handle(GetLeaguesRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getLeague = await MapToGetLeagueModelsAsync(cancellationToken);
        return getLeague;
    }

    public async Task<IEnumerable<LeagueModel>> MapToGetLeagueModelsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Leagues
            .Where(x => x.LeaguePublic == Common.Enums.LeaguePublicSetting.PublicListed)
            .Select(MapToGetLeagueModelExpression)
            .ToListAsync(cancellationToken);
    }
}
