using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal sealed class StandingCalculationConfigurationProvider : DatabaseAccessBase, IStandingCalculationConfigurationProvider
{
    public StandingCalculationConfigurationProvider(LeagueDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<StandingCalculationConfiguration> GetConfiguration(long seasonId, long? eventId, long? resultConfigId, CancellationToken cancellationToken = default)
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
        var resultConfig = await GetConfigurationEntityAsync(resultConfigId, cancellationToken);
        if (resultConfig is not null)
        {
            config.ResultConfigId = resultConfig.ResultConfigId;
            config.Name = resultConfig.Name;
            config.DisplayName = resultConfig.DisplayName;
        }
        return config;
    }

    private async Task<SeasonEntity?> GetSeasonEntityAsync(long seasonId, CancellationToken cancellationToken)
    {
        return await dbContext.Seasons
            .FirstOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken);
    }

    private async Task<ResultConfigurationEntity?> GetConfigurationEntityAsync(long? resultConfigId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultConfigurations
            .FirstOrDefaultAsync(x => x.ResultConfigId == resultConfigId);
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
