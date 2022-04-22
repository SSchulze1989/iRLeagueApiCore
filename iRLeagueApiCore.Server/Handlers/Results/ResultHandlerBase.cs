using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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

        public Expression<Func<ScoredResultEntity, GetResultModel>> MapToGetResultModelExpression => result => new GetResultModel()
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
            ResultRows = result.ScoredResultRows.Select(row => new GetResultRowModel()
            {
                MemberId = row.ResultRow.MemberId,
                Interval = new TimeSpan(row.ResultRow.Interval),
                FastestLapTime = new TimeSpan(row.ResultRow.FastestLapTime),
                AvgLapTime = new TimeSpan(row.ResultRow.AvgLapTime),
                Firstname = row.ResultRow.Member.Firstname,
                Lastname = row.ResultRow.Member.Lastname,
                TeamName = row.Team.Name,
                StartPosition = row.ResultRow.StartPosition,
                FinishPosition = row.ResultRow.FinishPosition,
                FinalPosition = row.FinalPosition,
                RacePoints = row.RacePoints,
                PenaltyPoints = row.PenaltyPoints,
                BonusPoints = row.BonusPoints,
                TotalPoints = row.TotalPoints,
                Car = row.ResultRow.Car,
                CarClass = row.ResultRow.CarClass,
                CarId = row.ResultRow.CarId,
                CarNumber = row.ResultRow.CarNumber,
                ClassId = row.ResultRow.ClassId,
                CompletedLaps = row.ResultRow.CompletedLaps,
                CompletedPct = row.ResultRow.CompletedPct,
                Division = row.ResultRow.Division,
                FastLapNr = row.ResultRow.FastLapNr,
                FinalPositionChange = row.FinalPositionChange,
                Incidents = row.ResultRow.Incidents,
                LeadLaps = row.ResultRow.LeadLaps,
                License = row.ResultRow.License,
                NewIrating = row.ResultRow.NewIrating,
                NewLicenseLevel = row.ResultRow.NewLicenseLevel,
                NewSafetyRating = row.ResultRow.NewSafetyRating,
                OldIrating = row.ResultRow.OldIrating,
                OldLicenseLevel = row.ResultRow.OldLicenseLevel,
                OldSafetyRating = row.ResultRow.OldSafetyRating,
                PositionChange = row.ResultRow.PositionChange,
                QualifyingTime = new TimeSpan(row.ResultRow.QualifyingTime),
                SeasonStartIrating = row.ResultRow.SeasonStartIrating,
                Status = row.ResultRow.Status,
                TeamId = row.TeamId
            }),
            CreatedOn = result.CreatedOn,
            LastModifiedOn = result.LastModifiedOn
        };
    }
}
