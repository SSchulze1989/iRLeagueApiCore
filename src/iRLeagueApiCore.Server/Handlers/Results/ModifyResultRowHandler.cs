using iRLeagueApiCore.Common.Models.Results;
using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record ModifyResultRowRequest(long ResultRowId, ModRawResultRowModel Row, bool TriggerCalculation = false) : IRequest<ModRawResultRowModel>;

public class ModifyResultRowHandler : ResultHandlerBase<ModifyResultRowHandler, ModifyResultRowRequest, ModRawResultRowModel>
{
    private readonly IResultCalculationQueue calculationQueue;

    public ModifyResultRowHandler(ILogger<ModifyResultRowHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<ModifyResultRowRequest>> validators,
        IResultCalculationQueue calculationQueue) : base(logger, dbContext, validators)
    {
        this.calculationQueue = calculationQueue;
    }

    public override async Task<ModRawResultRowModel> Handle(ModifyResultRowRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var row = await GetResultRowEntityAsync(request.ResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        await MapToResultRowEntity(row, request.Row);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.TriggerCalculation)
        {
            var eventId = await dbContext.ResultRows
                .Where(x => x.ResultRowId == row.ResultRowId)
                .Select(x => x.SubResult.EventId)
                .FirstOrDefaultAsync(cancellationToken);
            await calculationQueue.QueueEventResultAsync(eventId);
        }

        return MapToModResultRowModel(request.Row, row);
    }

    private async Task<ResultRowEntity?> GetResultRowEntityAsync(long resultRowId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultRows.
            FirstOrDefaultAsync(x => resultRowId == x.ResultRowId, cancellationToken);
    }

    private async Task<ResultRowEntity> MapToResultRowEntity(ResultRowEntity entity, ModRawResultRowModel model)
    {
        entity.StartPosition = model.StartPosition;
        entity.FinishPosition = model.FinishPosition;
        entity.CarNumber = model.CarNumber;
        entity.ClassId = model.ClassId;
        entity.Car = model.Car;
        entity.CarClass = model.CarClass;
        entity.CompletedLaps = model.CompletedLaps;
        entity.LeadLaps = model.LeadLaps;
        entity.FastLapNr = model.FastLapNr;
        entity.Incidents = model.Incidents;
        entity.Status = model.Status;
        entity.QualifyingTime = model.QualifyingTime;
        entity.Interval = model.Interval;
        entity.AvgLapTime = model.AvgLapTime;
        entity.FastestLapTime = model.FastestLapTime;
        entity.PositionChange = model.PositionChange;
        entity.IRacingId = model.IRacingId;
        entity.SimSessionType = model.SimSessionType;
        entity.OldIRating = model.OldIRating;
        entity.NewIRating = model.NewIRating;
        entity.SeasonStartIRating = model.SeasonStartIRating;
        entity.License = model.License;
        entity.OldSafetyRating = model.OldSafetyRating;
        entity.NewSafetyRating = model.NewSafetyRating;
        entity.OldCpi = model.OldCpi;
        entity.NewCpi = model.NewCpi;
        entity.ClubId = model.ClubId;
        entity.ClubName = model.ClubName;
        entity.CarId = model.CarId;
        entity.CompletedPct = model.CompletedPct;
        entity.QualifyingTimeAt = model.QualifyingTimeAt;
        entity.Division = model.Division;
        entity.OldLicenseLevel = model.OldLicenseLevel;
        entity.NewLicenseLevel = model.NewLicenseLevel;
        entity.NumPitStops = model.NumPitStops;
        entity.PittedLaps = model.PittedLaps;
        entity.NumOfftrackLaps = model.NumOfftrackLaps;
        entity.OfftrackLaps = model.OfftrackLaps;
        entity.NumContactLaps = model.NumContactLaps;
        entity.ContactLaps = model.ContactLaps;
        entity.RacePoints = model.RacePoints;
        var team = await dbContext.Teams
            .FirstOrDefaultAsync(x => x.TeamId == model.TeamId);
        entity.Team = team;
        entity.PointsEligible = model.PointsEligible;
        return entity;
    }

    private static ModRawResultRowModel MapToModResultRowModel(ModRawResultRowModel model, ResultRowEntity entity)
    {
        model.StartPosition = entity.StartPosition;
        model.FinishPosition = entity.FinishPosition;
        model.CarNumber = entity.CarNumber;
        model.ClassId = entity.ClassId;
        model.Car = entity.Car;
        model.CarClass = entity.CarClass;
        model.CompletedLaps = entity.CompletedLaps;
        model.LeadLaps = entity.LeadLaps;
        model.FastLapNr = entity.FastLapNr;
        model.Incidents = entity.Incidents;
        model.Status = entity.Status;
        model.QualifyingTime = entity.QualifyingTime;
        model.Interval = entity.Interval;
        model.AvgLapTime = entity.AvgLapTime;
        model.FastestLapTime = entity.FastestLapTime;
        model.PositionChange = entity.PositionChange;
        model.IRacingId = entity.IRacingId;
        model.SimSessionType = entity.SimSessionType;
        model.OldIRating = entity.OldIRating;
        model.NewIRating = entity.NewIRating;
        model.SeasonStartIRating = entity.SeasonStartIRating;
        model.License = entity.License;
        model.OldSafetyRating = entity.OldSafetyRating;
        model.NewSafetyRating = entity.NewSafetyRating;
        model.OldCpi = entity.OldCpi;
        model.NewCpi = entity.NewCpi;
        model.ClubId = entity.ClubId;
        model.ClubName = entity.ClubName;
        model.CarId = entity.CarId;
        model.CompletedPct = entity.CompletedPct;
        model.QualifyingTimeAt = entity.QualifyingTimeAt;
        model.Division = entity.Division;
        model.OldLicenseLevel = entity.OldLicenseLevel;
        model.NewLicenseLevel = entity.NewLicenseLevel;
        model.NumPitStops = entity.NumPitStops;
        model.PittedLaps = entity.PittedLaps;
        model.NumOfftrackLaps = entity.NumOfftrackLaps;
        model.OfftrackLaps = entity.OfftrackLaps;
        model.NumContactLaps = entity.NumContactLaps;
        model.ContactLaps = entity.ContactLaps;
        model.RacePoints = entity.RacePoints;
        model.TeamId = entity.TeamId;
        model.PointsEligible = entity.PointsEligible;
        return model;
    }
}
