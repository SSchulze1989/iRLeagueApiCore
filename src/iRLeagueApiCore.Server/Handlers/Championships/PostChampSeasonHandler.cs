using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Threading;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record PostChampSeasonRequest(long LeagueId, long ChampionshipId, long SeasonId, LeagueUser User, PostChampSeasonModel Model) : IRequest<ChampSeasonModel>;

public sealed class PostChampSeasonHandler : ChampSeasonHandlerBase<PostChampSeasonHandler, PostChampSeasonRequest>, 
    IRequestHandler<PostChampSeasonRequest, ChampSeasonModel>
{
    public PostChampSeasonHandler(ILogger<PostChampSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostChampSeasonRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampSeasonModel> Handle(PostChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postChampSeason = await GetChampSeasonEntityAsync(request.LeagueId, request.ChampionshipId, request.SeasonId, cancellationToken)
            ?? await CreateChampSeasonEntityAsync(request.User, request.LeagueId, request.ChampionshipId, request.SeasonId, cancellationToken);
        postChampSeason.IsActive = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        var getChampSeason = await MapToChampSeasonModel(request.LeagueId, postChampSeason.ChampSeasonId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource not found");
        return getChampSeason;
    }

    private async Task<ChampSeasonEntity?> GetChampSeasonEntityAsync(long leagueId, long championshipId, long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampionshipId == championshipId)
            .Where(x => x.SeasonId == seasonId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ChampSeasonEntity> CreateChampSeasonEntityAsync(LeagueUser user, long leagueId, long championshipId, long seasonId, CancellationToken cancellationToken)
    {
        var championship = await dbContext.Championships
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.ChampionshipId == championshipId)
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.ResultConfigurations)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var season = await dbContext.Seasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SeasonId == seasonId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var champSeason = CreateVersionEntity(user, new ChampSeasonEntity()
        {
            Championship = championship,
            Season = season,
            IsActive = true,
        });
        var prevChampSeason = championship.ChampSeasons
            .LastOrDefault();

        // find previous season settings and copy
        champSeason.StandingConfiguration = await CreateOrCopyStandingConfigEntity(user, leagueId, prevChampSeason?.StandingConfigId, cancellationToken);
        champSeason.ResultConfigurations = await CopyResultConfigurationEntities(user, leagueId, prevChampSeason?.ResultConfigurations.Select(x => x.ResultConfigId), cancellationToken);
        UpdateVersionEntity(user, champSeason);

        championship.ChampSeasons.Add(champSeason);
        season.ChampSeasons.Add(champSeason);
        //dbContext.ChampSeasons.Add(champSeason);
        return champSeason;
    }

    private async Task<StandingConfigurationEntity> CreateOrCopyStandingConfigEntity(LeagueUser user, long leagueId, long? prevStandingConfigId, CancellationToken cancellationToken)
    {
        var target = CreateVersionEntity(user, new StandingConfigurationEntity()
        {
            LeagueId = leagueId,
        });
        var source = await dbContext.StandingConfigurations
            .Where(x => x.LeagueId == leagueId)
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
            Conditions = source.Conditions.Select(x => new FilterConditionEntity()
            {
                Action = x.Action,
                ColumnPropertyName = x.ColumnPropertyName,
                Comparator = x.Comparator,
                FilterType = x.FilterType,
                FilterValues = x.FilterValues,
            }).ToList(),
        });
        UpdateVersionEntity(user, target);
        return target;
    }

    private PointRuleEntity CopyPointRuleEntity(LeagueUser user, PointRuleEntity source)
    {
        var target = CreateVersionEntity(user, new PointRuleEntity()
        {
            BonusPoints = source.BonusPoints,
            FinalSortOptions = source.FinalSortOptions,
            MaxPoints = source.MaxPoints,
            Name = source.Name,
            PointDropOff = source.PointDropOff,
            PointsPerPlace = source.PointsPerPlace,
            PointsSortOptions = source.PointsSortOptions,
        });
        UpdateVersionEntity(user, target);
        return target;
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

    private ResultConfigurationEntity CopyResultConfigurationEntity(LeagueUser user, ResultConfigurationEntity source)
    {
        var target = CreateVersionEntity(user, new ResultConfigurationEntity() 
        {
            DisplayName = source.DisplayName,
            Name = source.Name,
            ResultKind = source.ResultKind,
            ResultsPerTeam = source.ResultsPerTeam,
            SourceResultConfigId= source.SourceResultConfigId,
            ResultFilters = source.ResultFilters.Select(x => CopyFilterOptionEntity(user, x)).ToList(),
            PointFilters = source.PointFilters.Select(x => CopyFilterOptionEntity(user, x)).ToList(),
            Scorings = source.Scorings.Select(x => CopyScoringEntity(user, x)).ToList(),
        });
        UpdateVersionEntity(user, target);
        return target;
    }

    private async Task<ICollection<ResultConfigurationEntity>> CopyResultConfigurationEntities(LeagueUser user, long leagueId, IEnumerable<long>? prevResultConfigIds,
        CancellationToken cancellationToken)
    {
        if (prevResultConfigIds is null)
        {
            return new List<ResultConfigurationEntity>();
        }

        var resultConfigs = await dbContext.ResultConfigurations
            .Where(x => x.LeagueId == leagueId)
            .Where(x => prevResultConfigIds.Contains(x.ResultConfigId))
            .Include(x => x.SourceResultConfig)
            .Include(x => x.PointFilters)
                .ThenInclude(x => x.Conditions)
            .Include(x => x.Scorings)
                .ThenInclude(x => x.PointsRule)
            .Include(x => x.ResultFilters)
                .ThenInclude(x => x.Conditions)
            .ToListAsync(cancellationToken);
        return resultConfigs;
    }

}
