using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetPointSystemsFromSeasonRequest(long SeasonId) : IRequest<IEnumerable<PointSystemModel>>;

public class GetPointSystemsFromSeasonHandler : PointSystemHandlerBase<GetPointSystemsFromSeasonHandler, GetPointSystemsFromSeasonRequest, IEnumerable<PointSystemModel>>
{
    public GetPointSystemsFromSeasonHandler(ILogger<GetPointSystemsFromSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetPointSystemsFromSeasonRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<PointSystemModel>> Handle(GetPointSystemsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var configs = await GetResultConfigsFromSeasonAsync(request.SeasonId, cancellationToken);
        return configs;
    }

    private async Task<IEnumerable<PointSystemModel>> GetResultConfigsFromSeasonAsync(long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.SeasonId == seasonId)
            .Where(x => x.IsActive)
            .SelectMany(x => x.PointSystems)
            .OrderBy(x => x.PointSystemId)
            .Select(MapToResultConfigModelExpression)
            .ToListAsync(cancellationToken);
    }
}
