using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public sealed class ChampSeasonHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{
    public ChampSeasonHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<ChampSeasonEntity?> GetChampSeasonEntityAsync(long leagueId, long champSeasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampSeasonId == champSeasonId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual async Task<ChampSeasonEntity> MapToChampSeasonEntityAsync(long leagueId, PostChampSeasonModel model, ChampSeasonEntity target, 
        CancellationToken cancellationToken)
    {
        target.StandingConfiguration = await GetStandingConfigurationEntityAsync(leagueId, model.StandingConfigId, cancellationToken);
        target.ResultConfigurations = await MapToResultConfigurationListAsync(leagueId, model.ResultConfigIds, target.ResultConfigurations, cancellationToken);
        return await Task.FromResult(target);
    }

    protected async Task<StandingConfigurationEntity?> GetStandingConfigurationEntityAsync(long leagueId, long? standingConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.StandingConfigurations
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.StandingConfigId == standingConfigId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected async Task<ICollection<ResultConfigurationEntity>> MapToResultConfigurationListAsync(long leagueId, IEnumerable<long> resultConfigId, 
        ICollection<ResultConfigurationEntity> target, CancellationToken cancellationToken)
    {
        target = await dbContext.ResultConfigurations
            .Where(x => x.LeagueId == leagueId)
            .Where(x => resultConfigId.Contains(x.ResultConfigId))
            .ToListAsync(cancellationToken);
        return target;
    }

    protected async Task<ChampSeasonModel?> MapToChampSeasonModel(long leagueId, long champSeasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampSeasonId == champSeasonId)
            .OrderBy(x => x.Season.SeasonStart)
            .Select(MapToChampSeasonModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual Expression<Func<ChampSeasonEntity, ChampSeasonModel>> MapToChampSeasonModelExpression => champSeason => new()
    {
        ChampionshipId = champSeason.ChampionshipId,
        ChampSeasonId = champSeason.ChampSeasonId,
        ResultConfigIds = champSeason.ResultConfigurations.Select(x => x.ResultConfigId).ToList(),
        SeasonId = champSeason.SeasonId,
        StandingConfigId = champSeason.StandingConfigId,
    };
}
