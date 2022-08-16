using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public class ResultHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public ResultHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public Expression<Func<ScoredResultEntity, ResultModel>> MapToGetResultModelExpression => result => new ResultModel()
        {
            LeagueId = result.LeagueId,
            SeasonId = result.Result.Session.Schedule.Season.SeasonId,
            SeasonName = result.Result.Session.Schedule.Season.SeasonName,
            ScheduleId = result.Result.Session.Schedule.ScheduleId,
            ScheduleName = result.Result.Session.Schedule.Name,
            ScoringId = result.ScoringId,
            ScoringName = result.Scoring.Name,
            SessionId = result.ResultId,
            SessionName = result.Result.Session.Name,
            ResultRows = result.ScoredResultRows.Select(row => new ResultRowModel()
            {
                MemberId = row.MemberId,
                Interval = new TimeSpan(row.Interval),
                FastestLapTime = new TimeSpan(row.FastestLapTime),
                AvgLapTime = new TimeSpan(row.AvgLapTime),
                Firstname = row.Member.Firstname,
                Lastname = row.Member.Lastname,
                TeamName = row.Team.Name,
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
                QualifyingTime = new TimeSpan(row.QualifyingTime),
                SeasonStartIrating = row.SeasonStartIRating,
                Status = row.Status,
                TeamId = row.TeamId
            }),
            CreatedOn = result.CreatedOn,
            LastModifiedOn = result.LastModifiedOn
        };
    }
}
