﻿using iRLeagueApiCore.Common.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public class ResultHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{
    public ResultHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public Expression<Func<ScoredEventResultEntity, EventResultModel>> MapToEventResultModelExpression => result => new EventResultModel()
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
        SessionResults = result.ScoredSessionResults.Select(sessionResult => new ResultModel()
        {
            LeagueId = sessionResult.LeagueId,
            ScoringId = sessionResult.ScoringId,
            SessionName = sessionResult.Name,
            SessionNr = sessionResult.SessionNr,
            ResultRows = sessionResult.ScoredResultRows.Select(row => new ResultRowModel()
            {
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
                Status = row.Status,
                TeamId = row.TeamId,
                TeamColor = (row.Team != null) ? row.Team.TeamColor : string.Empty,
                //TeamResultRows = row.TeamResultRows.Select(teamRow => new ResultRowModel()
                //{
                //    MemberId = row.MemberId,
                //    Interval = new TimeSpan(row.Interval),
                //    FastestLapTime = new TimeSpan(row.FastestLapTime),
                //    AvgLapTime = new TimeSpan(row.AvgLapTime),
                //    Firstname = (row.Member != null) ? row.Member.Firstname : string.Empty,
                //    Lastname = (row.Member != null) ? row.Member.Lastname : string.Empty,
                //    TeamName = (row.Team != null) ? row.Team.Name : string.Empty,
                //    StartPosition = row.StartPosition,
                //    FinishPosition = row.FinishPosition,
                //    FinalPosition = row.FinalPosition,
                //    RacePoints = row.RacePoints,
                //    PenaltyPoints = row.PenaltyPoints,
                //    BonusPoints = row.BonusPoints,
                //    TotalPoints = row.TotalPoints,
                //    Car = row.Car,
                //    CarClass = row.CarClass,
                //    CarId = row.CarId,
                //    CarNumber = row.CarNumber,
                //    ClassId = row.ClassId,
                //    CompletedLaps = row.CompletedLaps,
                //    CompletedPct = row.CompletedPct,
                //    Division = row.Division,
                //    FastLapNr = row.FastLapNr,
                //    FinalPositionChange = row.FinalPositionChange,
                //    Incidents = row.Incidents,
                //    LeadLaps = row.LeadLaps,
                //    License = row.License,
                //    NewIrating = row.NewIRating,
                //    NewLicenseLevel = row.NewLicenseLevel,
                //    NewSafetyRating = row.NewSafetyRating,
                //    OldIrating = row.OldIRating,
                //    OldLicenseLevel = row.OldLicenseLevel,
                //    OldSafetyRating = row.OldSafetyRating,
                //    PositionChange = row.PositionChange,
                //    QualifyingTime = new TimeSpan(row.QualifyingTime),
                //    SeasonStartIrating = row.SeasonStartIRating,
                //    Status = row.Status,
                //    TeamId = row.TeamId,
                //}),
            }),
            CreatedOn = sessionResult.CreatedOn,
            LastModifiedOn = sessionResult.LastModifiedOn,
        }),
    };
}
