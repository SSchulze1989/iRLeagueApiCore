using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal sealed class StandingCalculationConfigurationProvider : DatabaseAccessBase, IStandingCalculationConfigurationProvider
{
    public StandingCalculationConfigurationProvider(LeagueDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<long?> GetSeasonId(long eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Events
            .Where(x => x.EventId == eventId)
            .Select(x => x.Schedule.SeasonId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<long>> GetStandingConfigIds(long seasonId, CancellationToken cancellationToken = default)
    {
        var configIds = await dbContext.ChampSeasons
            .Where(x => x.SeasonId == seasonId)
            .Where(x => x.StandingConfigId != null)
            .Select(x => x.StandingConfigId.GetValueOrDefault())
            .Distinct()
            .ToListAsync(cancellationToken);
        return configIds;
    }

    public async Task<StandingCalculationConfiguration> GetConfiguration(long seasonId, long? eventId, long? standingConfigId, CancellationToken cancellationToken = default)
    {
        var season = await GetSeasonEntityAsync(seasonId, cancellationToken)
            ?? throw new ArgumentException($"No season with id {seasonId} found", nameof(seasonId));
        var @event = await GetEventEntityAsync(seasonId, eventId, cancellationToken)
            ?? await GetLatestEventEntityAsync(seasonId, cancellationToken);
        if (@event is null)
        {
            return EmptyStandingConfiguration();
        }
        var config = DefaultStandingConfiguration(season, @event.EventId);
        var standingConfig = await GetConfigurationEntityAsync(standingConfigId, cancellationToken);
        var champSeason = standingConfig?.ChampSeasons.FirstOrDefault(x => x.SeasonId == seasonId);
        if (standingConfig is not null && champSeason is not null)
        {
            config.StandingConfigId = standingConfig.StandingConfigId;
            config.ResultConfigId = champSeason.ResultConfigurations.FirstOrDefault()?.ResultConfigId;
            config.Name = champSeason.ResultConfigurations.FirstOrDefault()?.Name ?? standingConfig.Name;
            config.DisplayName = champSeason.ResultConfigurations.FirstOrDefault()?.DisplayName ?? standingConfig.Name;
            config.UseCombinedResult = standingConfig.UseCombinedResult;
            config.ResultKind = champSeason.ResultConfigurations.FirstOrDefault()?.ResultKind ?? standingConfig.ResultKind;
            config.WeeksCounted = standingConfig.WeeksCounted;
        }
        return config;
    }

    private async Task<SeasonEntity?> GetSeasonEntityAsync(long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.Seasons
            .FirstOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken);
    }

    private async Task<StandingConfigurationEntity?> GetConfigurationEntityAsync(long? standingConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.StandingConfigurations
            .Include(x => x.ChampSeasons)
                .ThenInclude(x => x.ResultConfigurations)
            .FirstOrDefaultAsync(x => x.StandingConfigId == standingConfigId, cancellationToken: cancellationToken);
    }

    private async Task<EventEntity?> GetEventEntityAsync(long seasonId, long? eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Events
            .Where(x => x.Schedule.SeasonId == seasonId)
            .FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
    }

    private async Task<EventEntity?> GetLatestEventEntityAsync(long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.Events
            .Where(x => x.Schedule.SeasonId == seasonId)
            .Where(x => x.ScoredEventResults.Any())
            .OrderBy(x => x.Date)
            .LastOrDefaultAsync(cancellationToken);
    }

    private static StandingCalculationConfiguration DefaultStandingConfiguration(SeasonEntity season, long eventId)
    {
        return new StandingCalculationConfiguration()
        {
            Name = "Default",
            DisplayName = "Default",
            LeagueId = season.LeagueId,
            SeasonId = season.SeasonId,
            EventId = eventId,
        };
    }

    private static StandingCalculationConfiguration EmptyStandingConfiguration()
    {
        return new StandingCalculationConfiguration();
    }
}
