using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetResultConfigsFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<ResultConfigModel>>;

public class GetResultConfigsFromSeasonHandler : ResultConfigHandlerBase<GetResultConfigsFromSeasonHandler, GetResultConfigsFromSeasonRequest>,
    IRequestHandler<GetResultConfigsFromSeasonRequest, IEnumerable<ResultConfigModel>>
{
    public GetResultConfigsFromSeasonHandler(ILogger<GetResultConfigsFromSeasonHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetResultConfigsFromSeasonRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ResultConfigModel>> Handle(GetResultConfigsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var configs = await GetResultConfigsFromSeasonAsync(request.LeagueId, request.SeasonId, cancellationToken);
        return configs;
    }

    private async Task<IEnumerable<ResultConfigModel>> GetResultConfigsFromSeasonAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SeasonId == seasonId)
            .Where(x => x.IsActive)
            .SelectMany(x => x.ResultConfigurations)
            .Select(MapToResultConfigModelExpression)
            .ToListAsync(cancellationToken);
    }
}
