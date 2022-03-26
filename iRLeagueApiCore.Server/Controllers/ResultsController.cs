using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("/{leagueName}/Result")]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    public class ResultsController : LeagueApiController
    {
        private readonly LeagueDbContext _dbContext;
        private readonly ILogger<ResultsController> _logger;

        public ResultsController(ILogger<ResultsController> logger, LeagueDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private static Expression<Func<ScoredResultEntity, GetResultModel>> GetResultModelFromDbExpression => result => new GetResultModel()
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

        [HttpGet]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        public async Task<ActionResult<IEnumerable<GetResultModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromQuery] long[] ids)
        {
            _logger.LogInformation("Get results from {LeagueName} for ids {ResultIds} by {Username}", leagueName, ids,
                User.Identity.Name);

            var dbResults = _dbContext.ScoredResults
                .Where(x => x.Result.LeagueId == leagueId)
                .Where(x => x.Scoring.ShowResults);

            if (ids != null && ids.Count() > 0)
            {
                dbResults = dbResults
                    .Where(x => ids.Contains(x.ResultId));
            }

            var getResult = await dbResults
                .Select(GetResultModelFromDbExpression)
                .ToListAsync();

            if (getResult.Count() == 0)
            {
                _logger.LogInformation("No Results found in {LeagueName} for ids {ResultIds}", leagueName, ids);
                return NotFound();
            }

            _logger.LogInformation("Return {Count} result entries from {LeagueName} for ids {ResultIds}", getResult.Count(), leagueName, ids);

            return Ok(getResult);
        }

        [HttpGet("FromSeason")]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        public async Task<ActionResult<IEnumerable<GetResultModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromQuery] long id)
        {
            _logger.LogInformation("Get results from {LeagueName} for season id {SeasonId} by {Username}", leagueName, id,
                User.Identity.Name);

            var dbResults = _dbContext.ScoredResults
                .Where(x => x.LeagueId == leagueId);

            dbResults = dbResults
                .Where(x => x.Result.Session.Schedule.SeasonId == id);

            var getResult = await dbResults
                .Select(GetResultModelFromDbExpression)
                .ToListAsync();

            if (getResult.Count() == 0)
            {
                _logger.LogInformation("No Results found in {LeagueName} for season id {SeasonId}", leagueName, id);
                return NotFound();
            }

            _logger.LogInformation("Return {Count} result entries from {LeagueName} for season id {SeasonId}", getResult.Count(), leagueName, id);

            return Ok(getResult);
        }

        [HttpGet("FromSession")]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        public async Task<ActionResult<IEnumerable<GetResultModel>>> GetFromSession([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromQuery] long id)
        {
            _logger.LogInformation("Get results from {LeagueName} for session id {SessionId} by {Username}", leagueName, id,
                User.Identity.Name);

            var dbResults = _dbContext.ScoredResults
                .Where(x => x.LeagueId == leagueId);

            dbResults = dbResults
                .Where(x => x.Result.ResultId == id);

            var getResult = await dbResults
                .Select(GetResultModelFromDbExpression)
                .ToListAsync();

            if (getResult.Count() == 0)
            {
                _logger.LogInformation("No Results found in {LeagueName} for session id {SessionId}", leagueName, id);
                return NotFound();
            }

            _logger.LogInformation("Return {Count} result entries from {LeagueName} for session id {SessionId}", getResult.Count(), leagueName, id);

            return Ok(getResult);
        }
    }
}
