using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.TrackImport.Models;
using iRLeagueApiCore.TrackImport.Service;

namespace iRLeagueApiCore.Server.Handlers.Tracks;

public record ImportTracksCommand() : IRequest;

public sealed class ImportTracksHandler : HandlerBase<ImportTracksHandler, ImportTracksCommand, Unit>
{
    private readonly TrackImportService trackImportService;

    public ImportTracksHandler(ILogger<ImportTracksHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<ImportTracksCommand>> validators, TrackImportService trackImportService) : base(logger, dbContext, validators)
    {
        this.trackImportService = trackImportService;
    }

    public override async Task<Unit> Handle(ImportTracksCommand request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var importTracks = await trackImportService.GetTracksData(cancellationToken);
        await UpdateTracksInDatabase(dbContext, importTracks, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task UpdateTracksInDatabase(LeagueDbContext dbContext, IEnumerable<Aydsko.iRacingData.Tracks.Track> importTracks, CancellationToken cancellationToken)
    {
        await dbContext.TrackGroups.LoadAsync(cancellationToken);

        foreach (var importTrack in importTracks)
        {
            var trackId = importTrack.TrackId;
            var trackName = importTrack.TrackName;
            var trackConfig = dbContext.TrackConfigs.Local
                .FirstOrDefault(x => x.TrackId == trackId)
                ?? await dbContext.TrackConfigs
                .Include(x => x.TrackGroup)
                .FirstOrDefaultAsync(x => x.TrackId == trackId, cancellationToken);
            var trackGroup = trackConfig?.TrackGroup
                ?? dbContext.TrackGroups.Local.FirstOrDefault(x => x.TrackName == trackName)
                ?? await dbContext.TrackGroups.FirstOrDefaultAsync(x => x.TrackName == trackName, cancellationToken);
            if (trackGroup == null)
            {
                _logger.LogInformation("Track {TrackName} does not exist and will be created", trackName);
                trackGroup = new TrackGroupEntity()
                {
                    Location = importTrack.Location,
                    TrackName = trackName,
                };
                dbContext.TrackGroups.Add(trackGroup);
            }
            if (trackConfig == null)
            {
                _logger.LogInformation("TrackConfig with id: {TrackId} does not exist and will be created", trackId);
                trackConfig = new TrackConfigEntity()
                {
                    TrackGroup = trackGroup,
                    TrackId = trackId,
                };
                dbContext.TrackConfigs.Add(trackConfig);
            }

            trackConfig = MapToTrackConfigEntity(importTrack, trackConfig);
            _logger.LogInformation("Updated data for track id: {TrackId}, track: {TrackName}, config: {ConfigName}",
                trackConfig.TrackId, trackConfig.TrackGroup.TrackName, trackConfig.ConfigName);
        }
    }

    private static TrackConfigEntity MapToTrackConfigEntity(Aydsko.iRacingData.Tracks.Track importTrack, TrackConfigEntity entity)
    {
        entity.ConfigName = importTrack.ConfigName ?? "-";
        entity.ConfigType = GetConfigType(importTrack.TrackTypes?[0].TrackType ?? string.Empty);
        entity.HasNightLighting = importTrack.NightLighting;
        entity.LengthKm = decimal.ToDouble(importTrack.TrackConfigLength) * TrackImportService.m2km;
        entity.Turns = importTrack.CornersPerLap;
        return entity;
    }

    private static ConfigType GetConfigType(string typeString)
    {
        switch (typeString)
        {
            case "road":
                return ConfigType.Road;
            case "oval":
                return ConfigType.Oval;
            case "dirt_oval":
                return ConfigType.DirtOval;
            case "dirt_road":
                return ConfigType.DirtRoad;
            default:
                return ConfigType.Unknown;
        }
    }
}
