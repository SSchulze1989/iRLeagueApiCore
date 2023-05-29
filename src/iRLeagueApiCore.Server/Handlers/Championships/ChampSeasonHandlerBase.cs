﻿using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public class ChampSeasonHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{    public ChampSeasonHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<ChampSeasonEntity?> GetChampSeasonEntityAsync(long champSeasonId, CancellationToken cancellationToken)
    {
        return await ChampSeasonsQuery()
            .Include(x => x.Championship)
            .Include(x => x.StandingConfiguration)
            .Include(x => x.ResultConfigurations)
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
        return await Task.FromResult(target);
    }

    protected async Task<StandingConfigurationEntity?> MapToStandingConfigurationAsync(LeagueUser user, StandingConfigModel? model, CancellationToken cancellationToken)
    {
        if (model is null)
        {
            return null;
        }

        var entity =  await dbContext.StandingConfigurations
            .Where(x => x.StandingConfigId == model.StandingConfigId && model.StandingConfigId != 0)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            entity = CreateVersionEntity(user, new StandingConfigurationEntity()
            {
                LeagueId = dbContext.LeagueProvider.LeagueId,
            });
        }
        entity.Name = model.Name;
        entity.ResultKind = model.ResultKind;
        entity.UseCombinedResult = model.UseCombinedResult;
        entity.WeeksCounted = model.WeeksCounted;
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
        ResultConfigs = champSeason.ResultConfigurations.Select(config => new ResultConfigInfoModel()
        {
            LeagueId = champSeason.LeagueId,
            ResultConfigId = config.ResultConfigId,
            ChampSeasonId = champSeason.ChampSeasonId,
            ChampionshipName = champSeason.Championship.Name,
            Name = config.Name,
            DisplayName = config.DisplayName,
        }).ToList(),
        SeasonId = champSeason.SeasonId,
        SeasonName = champSeason.Season.SeasonName,
        StandingConfig = champSeason.StandingConfiguration == null ? null : new StandingConfigModel()
        {
            StandingConfigId = champSeason.StandingConfiguration.StandingConfigId,
            Name = champSeason.StandingConfiguration.Name,
            ResultKind = champSeason.StandingConfiguration.ResultKind,
            UseCombinedResult = champSeason.StandingConfiguration.UseCombinedResult,
            WeeksCounted = champSeason.StandingConfiguration.WeeksCounted,
        },
    };
}
