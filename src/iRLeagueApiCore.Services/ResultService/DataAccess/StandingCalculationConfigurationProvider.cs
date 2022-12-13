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
        var configIds = await dbContext.Seasons
            .Where(x => x.SeasonId == seasonId)
            .SelectMany(x => x.Schedules)
            .SelectMany(x => x.Events)
            .SelectMany(x => x.ResultConfigs)
            .SelectMany(x => x.StandingConfigurations)
            .Select(x => x.StandingConfigId)
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
        if (standingConfig is not null)
        {
            config.StandingConfigId = standingConfig.StandingConfigId;
            config.ResultConfigId = standingConfig.ResultConfigurations.FirstOrDefault()?.ResultConfigId;
            config.Name = standingConfig.ResultConfigurations.FirstOrDefault()?.Name ?? standingConfig.Name;
            config.DisplayName = standingConfig.ResultConfigurations.FirstOrDefault()?.DisplayName ?? standingConfig.Name;
            config.UseCombinedResult = standingConfig.UseCombinedResult;
            config.ResultKind = standingConfig.ResultConfigurations.FirstOrDefault()?.ResultKind ?? standingConfig.ResultKind;
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
            .Include(x => x.ResultConfigurations)
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
