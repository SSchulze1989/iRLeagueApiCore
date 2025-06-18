using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public abstract class ChampSeasonHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public ChampSeasonHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<ChampSeasonEntity?> GetChampSeasonEntityAsync(long champSeasonId, CancellationToken cancellationToken)
    {
        return await ChampSeasonsQuery()
            .Include(x => x.Championship)
            .Include(x => x.StandingConfiguration)
            .Include(x => x.ResultConfigurations)
            .Include(x => x.DefaultResultConfig)
            .Include(x => x.Filters)
            .Where(x => x.ChampSeasonId == champSeasonId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual async Task<ChampSeasonEntity> MapToChampSeasonEntityAsync(LeagueUser user, PutChampSeasonModel model, ChampSeasonEntity target,
        CancellationToken cancellationToken)
    {
        target.Championship.Name = model.ChampionshipName;
        target.Championship.DisplayName = model.ChampionshipDisplayName;
        target.StandingConfiguration = await MapToStandingConfigurationAsync(user, model.StandingConfig, cancellationToken);
        target.ResultConfigurations = await MapToResultConfigurationListAsync(model.ResultConfigs.Select(x => x.ResultConfigId), target.ResultConfigurations, cancellationToken);
        target.DefaultResultConfig = target.ResultConfigurations.FirstOrDefault(x => x.ResultConfigId == model.DefaultResultConfig?.ResultConfigId);
        target.ResultKind = model.ResultKind;
        target.Filters = await MapToFilterOptionListAsync(user, model.Filters, target.Filters, cancellationToken);
        target.Index = model.Index;
        target.Roster = model.Roster is not null ? await dbContext.Rosters.FirstOrDefaultAsync(x => x.RosterId == model.Roster.RosterId, cancellationToken) : null;
        target.RosterId = target.Roster?.RosterId;
        return await Task.FromResult(target);
    }

    protected async Task<StandingConfigurationEntity?> MapToStandingConfigurationAsync(LeagueUser user, StandingConfigModel? model, CancellationToken cancellationToken)
    {
        if (model is null)
        {
            return null;
        }

        var entity = await dbContext.StandingConfigurations
            .Where(x => x.StandingConfigId == model.StandingConfigId && model.StandingConfigId != 0)
            .FirstOrDefaultAsync(cancellationToken);
        entity ??= CreateVersionEntity(user, new StandingConfigurationEntity()
        {
            LeagueId = dbContext.LeagueProvider.LeagueId,
        });
        entity.Name = model.Name;
        entity.ResultKind = model.ResultKind;
        entity.UseCombinedResult = model.UseCombinedResult;
        entity.WeeksCounted = model.WeeksCounted;
        entity.SortOptions = model.SortOptions;
        UpdateVersionEntity(user, entity);
        return entity;
    }

    protected async Task<ICollection<ResultConfigurationEntity>> MapToResultConfigurationListAsync(IEnumerable<long> resultConfigId,
        ICollection<ResultConfigurationEntity> target, CancellationToken cancellationToken)
    {
        target = await dbContext.ResultConfigurations
            .Where(x => resultConfigId.Contains(x.ResultConfigId))
            .ToListAsync(cancellationToken);
        return target;
    }

    protected async Task<ChampSeasonModel?> MapToChampSeasonModel(long champSeasonId, CancellationToken cancellationToken)
    {
        return await ChampSeasonsQuery()
            .Where(x => x.ChampSeasonId == champSeasonId)
            .Select(MapToChampSeasonModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected IQueryable<ChampSeasonEntity> ChampSeasonsQuery()
    {
        return dbContext.ChampSeasons
            .Where(x => x.IsActive);
    }

    protected virtual Expression<Func<ChampSeasonEntity, ChampSeasonModel>> MapToChampSeasonModelExpression => champSeason => new()
    {
        ChampionshipId = champSeason.ChampionshipId,
        ChampSeasonId = champSeason.ChampSeasonId,
        ChampionshipName = champSeason.Championship.Name,
        ChampionshipDisplayName = champSeason.Championship.DisplayName,
        ResultKind = champSeason.ResultKind,
        Index = champSeason.Index,
        Roster = champSeason.Roster == null ? null : new RosterInfoModel()
        {
            RosterId = champSeason.Roster.RosterId,
            Name = champSeason.Roster.Name,
            Description = champSeason.Roster.Description,
            EntryCount = champSeason.Roster.RosterEntries.Count,
        },
        ResultConfigs = champSeason.ResultConfigurations.Select(config => new ResultConfigInfoModel()
        {
            LeagueId = champSeason.LeagueId,
            ResultConfigId = config.ResultConfigId,
            ChampSeasonId = champSeason.ChampSeasonId,
            ChampionshipName = champSeason.Championship.Name,
            Name = config.Name,
            DisplayName = config.DisplayName,
            IsDefaultConfig = config.ResultConfigId == champSeason.DefaultResultConfigId,
        }).ToList(),
        DefaultResultConfig = champSeason.DefaultResultConfig == null ? null : new ResultConfigInfoModel()
        {
            LeagueId = champSeason.LeagueId,
            ResultConfigId = champSeason.DefaultResultConfig.ResultConfigId,
            ChampSeasonId = champSeason.ChampSeasonId,
            ChampionshipName = champSeason.Championship.Name,
            Name = champSeason.DefaultResultConfig.Name,
            DisplayName = champSeason.DefaultResultConfig.DisplayName,
            IsDefaultConfig = true,
        },
        SeasonId = champSeason.SeasonId,
        SeasonName = champSeason.Season.SeasonName,
        StandingConfig = champSeason.StandingConfiguration == null ? null : new StandingConfigModel()
        {
            StandingConfigId = champSeason.StandingConfiguration.StandingConfigId,
            Name = champSeason.StandingConfiguration.Name,
            ResultKind = champSeason.StandingConfiguration.ResultKind,
            UseCombinedResult = champSeason.StandingConfiguration.UseCombinedResult,
            WeeksCounted = champSeason.StandingConfiguration.WeeksCounted,
            SortOptions = champSeason.StandingConfiguration.SortOptions,
        },
        Filters = champSeason.Filters.Select(filter => new ResultFilterModel()
        {
            LeagueId = filter.LeagueId,
            FilterOptionId = filter.FilterOptionId,
            Condition = filter.Conditions.FirstOrDefault() ?? new(),
        }).ToList(),
    };
}
