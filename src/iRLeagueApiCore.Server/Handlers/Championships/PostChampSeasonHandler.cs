using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.ResultService.Extensions;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record PostChampSeasonRequest(long ChampionshipId, long SeasonId, LeagueUser User, PostChampSeasonModel Model) : IRequest<ChampSeasonModel>;

public sealed class PostChampSeasonHandler : ChampSeasonHandlerBase<PostChampSeasonHandler, PostChampSeasonRequest, ChampSeasonModel>
{
    public PostChampSeasonHandler(ILogger<PostChampSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostChampSeasonRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<ChampSeasonModel> Handle(PostChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postChampSeason = await GetChampSeasonEntityAsync(request.ChampionshipId, request.SeasonId, cancellationToken)
            ?? await CreateChampSeasonEntityAsync(request.User, request.ChampionshipId, request.SeasonId, cancellationToken);
        postChampSeason.IsActive = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        var getChampSeason = await MapToChampSeasonModel(postChampSeason.ChampSeasonId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource not found");
        return getChampSeason;
    }

    private async Task<ChampSeasonEntity?> GetChampSeasonEntityAsync(long championshipId, long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.ChampionshipId == championshipId)
            .Where(x => x.SeasonId == seasonId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ChampSeasonEntity> CreateChampSeasonEntityAsync(LeagueUser user, long championshipId, long seasonId, CancellationToken cancellationToken)
    {
        var championship = await dbContext.Championships
            .Where(x => x.ChampionshipId == championshipId)
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.PointSystems)
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.Season)
                    .ThenInclude(x => x.Schedules)
                        .ThenInclude(x => x.Events)
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.Filters)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var season = await dbContext.Seasons
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.PointSystems)
            .Where(x => x.SeasonId == seasonId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var prevChampSeason = championship.ChampSeasons
            .Select(x => new { ChampSeason = x, Events = x.Season.Schedules.SelectMany(x => x.Events) })
            .Where(x => x.Events.Any())
            .OrderByDescending(x => x.Events.Select(x => x.Date).Where(x => x < DateTime.UtcNow).Max())
            .Select(x => x.ChampSeason)
            .FirstOrDefault();
        var champSeason = CreateVersionEntity(user, new ChampSeasonEntity()
        {
            Championship = championship,
            Season = season,
            IsActive = true,
            ResultKind = prevChampSeason?.ResultKind ?? ResultKind.Member,
        });

        // copy previous season settings
        champSeason.StandingConfiguration = await CreateOrCopyStandingConfigEntity(user, prevChampSeason?.StandingConfigId, cancellationToken);
        champSeason.PointSystems = await CopyResultConfigurationEntities(user, champSeason, prevChampSeason?.PointSystems.Select(x => x.PointSystemId), cancellationToken);
        champSeason.Filters = CopyChampSeasonFilterEntities(user, prevChampSeason?.Filters);
        UpdateVersionEntity(user, champSeason);

        championship.ChampSeasons.Add(champSeason);
        season.ChampSeasons.Add(champSeason);
        //dbContext.ChampSeasons.Add(champSeason);
        return champSeason;
    }

    private ICollection<FilterOptionEntity> CopyChampSeasonFilterEntities(LeagueUser user, ICollection<FilterOptionEntity>? filters)
    {
        var copyFilters = filters?.Select(x => CreateVersionEntity(user, new FilterOptionEntity()
        {
            Conditions = x.Conditions,

        })) ?? new List<FilterOptionEntity>();
        copyFilters.ForEach(x => UpdateVersionEntity(user, x));
        return copyFilters.ToList();
    }

    private async Task<StandingConfigurationEntity> CreateOrCopyStandingConfigEntity(LeagueUser user, long? prevStandingConfigId, CancellationToken cancellationToken)
    {
        var target = CreateVersionEntity(user, new StandingConfigurationEntity()
        {
            LeagueId = dbContext.LeagueProvider.LeagueId,
        });
        var source = await dbContext.StandingConfigurations
            .Where(x => x.StandingConfigId == prevStandingConfigId)
            .FirstOrDefaultAsync(cancellationToken);
        if (source is not null)
        {
            target.Name = source.Name;
            target.ResultKind = source.ResultKind;
            target.UseCombinedResult = source.UseCombinedResult;
            target.WeeksCounted = source.WeeksCounted;
        }
        UpdateVersionEntity(user, target);
        return target;
    }

    private FilterOptionEntity CopyFilterOptionEntity(LeagueUser user, FilterOptionEntity source)
    {
        var target = CreateVersionEntity(user, new FilterOptionEntity()
        {
            Conditions = source.Conditions,
        });
        UpdateVersionEntity(user, target);
        return target;
    }

    private PointRuleEntity? CopyPointRuleEntity(LeagueUser user, PointRuleEntity? source)
    {
        if (source is null)
        {
            return null;
        }
        var target = CreateVersionEntity(user, new PointRuleEntity()
        {
            League = source.League,
            BonusPoints = source.BonusPoints,
            FinalSortOptions = source.FinalSortOptions,
            MaxPoints = source.MaxPoints,
            Name = source.Name,
            PointDropOff = source.PointDropOff,
            PointsPerPlace = source.PointsPerPlace,
            PointsSortOptions = source.PointsSortOptions,
            AutoPenalties = CopyAutoPenalties(source.AutoPenalties),
        });
        UpdateVersionEntity(user, target);
        return target;
    }

    private static ICollection<AutoPenaltyConfigEntity> CopyAutoPenalties(ICollection<AutoPenaltyConfigEntity> autoPenalties)
    {
        return autoPenalties.Select(x => new AutoPenaltyConfigEntity()
        {
            Conditions = x.Conditions,
            Description = x.Description,
            Points = x.Points,
            Positions = x.Positions,
            Time = x.Time,
            Type = x.Type,
        }).ToList();
    }

    private ScoringEntity CopyScoringEntity(LeagueUser user, ScoringEntity source)
    {
        var target = CreateVersionEntity(user, new ScoringEntity()
        {
            IsCombinedResult = source.IsCombinedResult,
            Index = source.Index,
            Name = source.Name,
            UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation,
            UseExternalSourcePoints = source.UseExternalSourcePoints,
            UseResultSetTeam = source.UseResultSetTeam,
            MaxResultsPerGroup = source.MaxResultsPerGroup,
            ShowResults = source.ShowResults,
            PointsRule = CopyPointRuleEntity(user, source.PointsRule),
        });
        UpdateVersionEntity(user, target);
        return target;
    }

    private PointSystemEntity CopyResultConfigurationEntity(LeagueUser user, ChampSeasonEntity currentChampSeason, PointSystemEntity source)
    {
        var target = CreateVersionEntity(user, new PointSystemEntity()
        {
            DisplayName = source.DisplayName,
            Name = source.Name,
            ResultsPerTeam = source.ResultsPerTeam,
            SourcePointSystem = GetSourceResultConfigurationEntity(currentChampSeason, source),
            ResultFilters = source.ResultFilters.Select(x => CopyFilterOptionEntity(user, x)).ToList(),
            PointFilters = source.PointFilters.Select(x => CopyFilterOptionEntity(user, x)).ToList(),
            Scorings = source.Scorings.Select(x => CopyScoringEntity(user, x)).ToList(),
        });
        UpdateVersionEntity(user, target);
        return target;
    }

    private PointSystemEntity? GetSourceResultConfigurationEntity(ChampSeasonEntity currentChampSeason, PointSystemEntity config)
    {
        if (config.SourcePointSystem is null)
        {
            return null;
        }
        var availableConfigs = currentChampSeason.Season.ChampSeasons.SelectMany(x => x.PointSystems);
        var sourceConfig = availableConfigs.FirstOrDefault(x => x.PointSystemId == config.SourcePointSystemId)
            ?? availableConfigs
                .Where(x => x.Name == config.SourcePointSystem.Name)
                .Where(x => x.ChampSeason.Championship.Name == config.SourcePointSystem.ChampSeason.Championship.Name)
                .FirstOrDefault();
        return sourceConfig;
    }

    private async Task<ICollection<PointSystemEntity>> CopyResultConfigurationEntities(LeagueUser user, ChampSeasonEntity targetChampSeason, IEnumerable<long>? prevResultConfigIds,
        CancellationToken cancellationToken)
    {
        if (prevResultConfigIds is null)
        {
            return new List<PointSystemEntity>();
        }


        var resultConfigs = await dbContext.ResultConfigurations
            .Where(x => prevResultConfigIds.Contains(x.PointSystemId))
            .Include(x => x.League)
            .Include(x => x.ChampSeason)
                .ThenInclude(x => x.Season)
            .Include(x => x.ChampSeason)
                .ThenInclude(x => x.Championship)
            .Include(x => x.SourcePointSystem)
                .ThenInclude(x => x.ChampSeason)
                    .ThenInclude(x => x.Championship)
            .Include(x => x.PointFilters)
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
                    .ThenInclude(x => x.AutoPenalties)
            .Include(x => x.ResultFilters)
            .ToListAsync(cancellationToken);
        return resultConfigs.Select(x => CopyResultConfigurationEntity(user, targetChampSeason, x)).ToList();
    }

}
