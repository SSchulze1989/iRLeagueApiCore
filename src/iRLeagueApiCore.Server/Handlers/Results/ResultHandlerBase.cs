using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public abstract class ResultHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public ResultHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    protected async Task<IEnumerable<EventResultModel>> MapToGetResultModelsFromEventAsync(long eventId, CancellationToken cancellationToken)
    {
        return await dbContext.ScoredEventResults
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.ResultConfigId)
            .Select(MapToEventResultModelExpression)
            .OrderBy(x => x.Index)
            .ToListAsync(cancellationToken);
    }

    protected async Task<ResultRowEntity> MapToResultRowEntity(ResultRowEntity entity, RawResultRowModel model, CancellationToken cancellationToken)
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
        entity.Status = (int)model.Status;
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
        entity.MemberId = model.MemberId;
        entity.Member = await dbContext.Members
            .FirstOrDefaultAsync(x => x.Id == model.MemberId, cancellationToken);
        var team = await dbContext.Teams
            .FirstOrDefaultAsync(x => x.TeamId == model.TeamId, cancellationToken);
        entity.TeamId = model.TeamId;
        entity.Team = team;
        entity.PointsEligible = model.PointsEligible;
        entity.CountryCode = model.CountryCode;
        return entity;
    }

    protected static RawResultRowModel MapToModResultRowModel(RawResultRowModel model, ResultRowEntity entity)
    {
        model.ResultRowId = entity.ResultRowId;
        model.MemberId = entity.MemberId;
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
        model.Status = (RaceStatus)entity.Status;
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
        model.CountryCode = entity.CountryCode;
        return model;
    }

    public static Expression<Func<ScoredEventResultEntity, EventResultModel>> MapToEventResultModelExpression => result => new EventResultModel()
    {
        EventId = result.EventId,
        LeagueId = result.LeagueId,
        EventName = result.Event.Name,
        DisplayName = result.Name,
        ResultId = result.ResultId,
        SeasonId = result.Event.Schedule.SeasonId,
        Date = result.Event.Date.GetValueOrDefault(),
        TrackId = result.Event.TrackId.GetValueOrDefault(),
        TrackName = result.Event.Track.TrackGroup.TrackName,
        ConfigName = result.Event.Track.ConfigName,
        StrengthOfField = result.Event.SimSessionDetails.Any() ? result.Event.SimSessionDetails.First().EventStrengthOfField : 0,
        Index = result.ChampSeason != null ? result.ChampSeason.Index : 0,
        SessionResults = result.ScoredSessionResults
            .OrderBy(sessionResult => sessionResult.SessionNr)
            .Select(sessionResult => new ResultModel()
            {
                LeagueId = sessionResult.LeagueId,
                ScoringId = sessionResult.ScoringId,
                SessionResultId = sessionResult.SessionResultId,
                SessionName = sessionResult.Name,
                SessionNr = sessionResult.SessionNr,
                ResultRows = sessionResult.ScoredResultRows
                    .OrderBy(x => x.FinalPosition)
                    .Select(row => new ResultRowModel()
                    {
                        ScoredResultRowId = row.ScoredResultRowId,
                        MemberId = row.MemberId,
                        Interval = new Interval(row.Interval),
                        FastestLapTime = row.FastestLapTime,
                        AvgLapTime = row.AvgLapTime,
                        Firstname = (row.Member != null) ? row.Member.Firstname : string.Empty,
                        Lastname = (row.Member != null) ? row.Member.Lastname : string.Empty,
                        TeamName = (row.Team != null) ? row.Team.Name : string.Empty,
                        StartPosition = row.StartPosition,
                        FinishPosition = row.FinishPosition,
                        FinalPosition = row.FinalPosition,
                        RacePoints = row.RacePoints,
                        PenaltyPoints = row.PenaltyPoints,
                        BonusPoints = row.BonusPoints,
                        TotalPoints = row.TotalPoints,
                        Car = row.Car,
                        CarClass = row.CarClass,
                        CarId = row.CarId,
                        CarNumber = row.CarNumber,
                        ClassId = row.ClassId,
                        ClubId = row.ClubId,
                        ClubName = row.ClubName,
                        CompletedLaps = row.CompletedLaps,
                        CompletedPct = row.CompletedPct,
                        Division = row.Division,
                        FastLapNr = row.FastLapNr,
                        FinalPositionChange = row.FinalPositionChange,
                        Incidents = row.Incidents,
                        LeadLaps = row.LeadLaps,
                        License = row.License,
                        NewIrating = row.NewIRating,
                        NewLicenseLevel = row.NewLicenseLevel,
                        NewSafetyRating = row.NewSafetyRating,
                        OldIrating = row.OldIRating,
                        OldLicenseLevel = row.OldLicenseLevel,
                        OldSafetyRating = row.OldSafetyRating,
                        PositionChange = row.PositionChange,
                        QualifyingTime = row.QualifyingTime,
                        SeasonStartIrating = row.SeasonStartIRating,
                        Status = (RaceStatus)row.Status,
                        TeamId = row.TeamId,
                        TeamColor = (row.Team != null) ? row.Team.TeamColor : string.Empty,
                        PenaltyTime = row.PenaltyTime,
                        PenaltyPositions = row.PenaltyPositions,
                        CountryCode = row.CountryCode,
                    }),
                FastestLapTime = sessionResult.FastestLap,
                FastestLapDriver = sessionResult.FastestLapDriver == null ? null : new()
                {
                    FirstName = sessionResult.FastestLapDriver.Firstname,
                    LastName = sessionResult.FastestLapDriver.Lastname,
                    MemberId = sessionResult.FastestLapDriver.Id,
                },
                PoleLapTime = sessionResult.FastestQualyLap,
                PoleLapDriver = sessionResult.FastestQualyLapDriver == null ? null : new()
                {
                    FirstName = sessionResult.FastestQualyLapDriver.Firstname,
                    LastName = sessionResult.FastestQualyLapDriver.Lastname,
                    MemberId = sessionResult.FastestQualyLapDriver.Id,
                },
                CleanestDrivers = sessionResult.CleanestDrivers.Select(member => new MemberInfoModel()
                {
                    FirstName = member.Firstname,
                    LastName = member.Lastname,
                    MemberId = member.Id,
                }).ToList(),
                CreatedOn = TreatAsUTCDateTime(sessionResult.CreatedOn),
                LastModifiedOn = TreatAsUTCDateTime(sessionResult.LastModifiedOn),
            }),
    };

    protected static Expression<Func<EventResultEntity, RawEventResultModel>> MapToRawEventResultModelExpression => result => new()
    {
        EventId = result.EventId,
        SessionResults = result.SessionResults
            .OrderBy(sessionResult => sessionResult.Session.SessionNr)
            .Select(sessionResult => new RawSessionResultModel()
            {
                SessionId = sessionResult.SessionId,
                ResultRows = sessionResult.ResultRows
                    .OrderBy(row => row.FinishPosition)
                    .Select(row => new RawResultRowModel()
                    {
                        ResultRowId = row.ResultRowId,
                        MemberId = row.MemberId,
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
                        Status = (RaceStatus)row.Status,
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
                        CountryCode = row.CountryCode,
                    }).ToList(),
            }).ToList(),
    };
}
