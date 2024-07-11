using iRLeagueApiCore.Common.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetRawResultsFromEventRequest(long EventId) : IRequest<RawEventResultModel>;

public class GetRawResultsFromEventHandler : ResultHandlerBase<GetRawResultsFromEventHandler, GetRawResultsFromEventRequest, RawEventResultModel>
{
    public GetRawResultsFromEventHandler(ILogger<GetRawResultsFromEventHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetRawResultsFromEventRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public override async Task<RawEventResultModel> Handle(GetRawResultsFromEventRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var result = await dbContext.EventResults
            .Where(x => x.EventId == request.EventId)
            .Select(MapToRawEventResultModelExpression)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        return result;
    }

    private Expression<Func<EventResultEntity, RawEventResultModel>> MapToRawEventResultModelExpression => result => new()
    {
        EventId = result.EventId,
        SessionResults = result.SessionResults.Select(sessionResult => new RawSessionResultModel()
        {
            SessionId = sessionResult.SessionId,
            ResultRows = sessionResult.ResultRows.Select(row => new RawResultRowModel()
            {
                StartPosition = row.StartPosition,
                FinishPosition = row.FinishPosition,
                CarNumber = row.CarNumber,
                ClassId = row.ClassId,
                Car = row.Car,
                CarClass = row.CarClass,
                CompletedLaps = row.CompletedLaps,
                LeadLaps = row.LeadLaps,
                FastLapNr = row.FastLapNr,
                Incidents = row.Incidents,
                Status = row.Status,
                QualifyingTime = row.QualifyingTime,
                Interval = row.Interval,
                AvgLapTime = row.AvgLapTime,
                FastestLapTime = row.FastestLapTime,
                PositionChange = row.PositionChange,
                IRacingId = row.IRacingId,
                SimSessionType = row.SimSessionType,
                OldIRating = row.OldIRating,
                NewIRating = row.NewIRating,
                SeasonStartIRating = row.SeasonStartIRating,
                License = row.License,
                OldSafetyRating = row.OldSafetyRating,
                NewSafetyRating = row.NewSafetyRating,
                OldCpi = row.OldCpi,
                NewCpi = row.NewCpi,
                ClubId = row.ClubId,
                ClubName = row.ClubName,
                CarId = row.CarId,
                CompletedPct = row.CompletedPct,
                QualifyingTimeAt = row.QualifyingTimeAt,
                Division = row.Division,
                OldLicenseLevel = row.OldLicenseLevel,
                NewLicenseLevel = row.NewLicenseLevel,
                NumPitStops = row.NumPitStops,
                PittedLaps = row.PittedLaps,
                NumOfftrackLaps = row.NumOfftrackLaps,
                OfftrackLaps = row.OfftrackLaps,
                NumContactLaps = row.NumContactLaps,
                ContactLaps = row.ContactLaps,
                RacePoints = row.RacePoints,
                TeamId = row.TeamId,
                PointsEligible = row.PointsEligible,
            }).ToList(),
        }).ToList(),
    };
}
