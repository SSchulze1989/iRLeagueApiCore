using iRLeagueApiCore.Common.Models.Championships;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public class ChampionshipHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{
    public ChampionshipHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<ChampionshipEntity?> GetChampionshipEntityAsync(long leagueId, long championshipId, CancellationToken cancellationToken)
    {
        return await dbContext.Championships
            .Include(x => x.ChampSeasons)
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampionshipId == championshipId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual async Task<ChampionshipEntity> MapToChampionshipEntityAsync(LeagueUser user, PostChampionshipModel postModel, ChampionshipEntity target, 
        CancellationToken cancellationToken)
    {
        target.Name = postModel.Name;
        target.DisplayName = postModel.DisplayName;
        UpdateVersionEntity(user, target);
        return await Task.FromResult(target);
    }

    protected virtual async Task<ChampionshipModel?> MapToChampionshipModelAsync(long leagueId, long championshipId, CancellationToken cancellationToken)
    {
        return await dbContext.Championships
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampionshipId == championshipId)
            .Select(MapToChampionshipModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual Expression<Func<ChampionshipEntity, ChampionshipModel>> MapToChampionshipModelExpression => championship => new()
    {
        ChampionshipId = championship.ChampionshipId,
        Name= championship.Name,
        DisplayName = championship.DisplayName,
        Seasons = championship.ChampSeasons.Select(champSeason => new ChampSeasonModel()
        {
            ChampSeasonId = champSeason.ChampSeasonId,
            ChampionshipId = championship.ChampionshipId,
            SeasonId = champSeason.SeasonId,
            ResultConfigIds = champSeason.ResultConfigurations.Select(x => x.ResultConfigId).ToList(),
            StandingConfigId = champSeason.StandingConfigId,
        }).ToList(),
    };
}
