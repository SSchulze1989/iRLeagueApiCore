using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    /// <summary>
    /// Endpoint for managing session entries
    /// </summary>
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SessionsController : LeagueApiController
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly LeagueDbContext _dbContext;

        public SessionsController(ILogger<SessionsController> logger, LeagueDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private static Expression<Func<SessionEntity, GetSessionModel>> MapToSessionModelExpr { get; } = x => new GetSessionModel()
        {
            SessionId = x.SessionId,
            ScheduleId = x.ScheduleId,
            LeagueId = x.LeagueId,
            PracticeAttached = x.PracticeAttached ?? false,
            QualyAttached = x.QualyAttached ?? false,
            PracticeLength = x.PracticeLength,
            QualyLength = x.QualyLength,
            Date = x.Date,
            Duration = x.Duration,
            Laps = x.Laps ?? 0,
            RaceLength = x.RaceLength,
            Name = x.Name,
            SessionTitle = x.SessionTitle,
            //SessionType = x.SessionType,
            SubSessionIds = x.SubSessions.Select(x => x.SessionId),
            ParentSessionId = x.ParentSessionId,
            SubSessionNr = x.SubSessionNr,
            TrackId = x.TrackId,
            HasResult = x.Result != null,
            CreatedOn = x.CreatedOn,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByUserName = x.CreatedByUserName,
            LastModifiedOn = x.LastModifiedOn,
            LastModifiedByUserId = x.LastModifiedByUserId,
            LastModifiedByUserName = x.LastModifiedByUserName
        };


        [HttpGet]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        public async Task<ActionResult<IEnumerable<GetSessionModel>>> Get([FromRoute] string leagueName, [ParameterIgnore] long leagueId,
            [FromQuery] long[] ids)
        {
            _logger.LogInformation("Get sessions from {LeagueName} for ids {SessionIds} by {UserName}", leagueName, ids,
                User.Identity.Name);

            IQueryable<SessionEntity> dbSessions = _dbContext.Sessions
                .Where(x => x.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbSessions = dbSessions.Where(x => ids.Contains(x.SessionId));
            }

            if (dbSessions.Count() == 0)
            {
                _logger.LogInformation("No sessions found in {LeagueName} for ids {SessionIds}", leagueName, ids);
                return NotFound();
            }

            var getSessions = await dbSessions
                .Select(MapToSessionModelExpr)
                .ToListAsync();

            _logger.LogInformation("Return {Count} session entries from {LeagueName} for ids {SessionIds}", getSessions.Count(),
                leagueName, ids);
            return Ok(getSessions);
        }

        [HttpPut]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult<GetSessionModel>> Put([FromRoute] string leagueName, [ParameterIgnore] long leagueId,
            [FromQuery] PutSessionModel putSession)
        {
            _logger.LogInformation("Put session data on {LeagueName} with id {SessionId} by {UserName}", leagueName,
                putSession.SessionId, User.Identity.Name);

            var dbSession = await _dbContext.Sessions
                .SingleOrDefaultAsync(x => x.SessionId == putSession.SessionId);

            ClaimsPrincipal currentUser = User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbSession == null)
            {
                _logger.LogInformation("Create session {SessionName}", putSession.Name);
                dbSession = new SessionEntity()
                {
                    LeagueId = leagueId,
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = currentUserID,
                    CreatedByUserName = User.Identity.Name
                };
                _dbContext.Sessions.Add(dbSession);
            }
            else if (dbSession.LeagueId != leagueId)
            {
                _logger.LogInformation("Session {SessionId} belongs to another league", putSession.SessionId);
                return BadRequestMessage("Session not found", $"No session with id {putSession.ScheduleId} could be found");
            }

            // update schedule if changed
            if (dbSession.ScheduleId != putSession.ScheduleId)
            {
                _logger.LogInformation("Move session {SessionId} to schedule {ScheduleId}", putSession.SessionId, putSession.ScheduleId);
                if (putSession.ScheduleId == null)
                {
                    _dbContext.Entry(dbSession)
                        .Reference(x => x.Schedule)
                        .Load();

                    dbSession.Schedule = null;
                    // dont delete session
                    _dbContext.Entry(dbSession)
                        .State = EntityState.Modified;
                }
                else
                {
                    var schedule = await _dbContext.Schedules
                    .Include(x => x.Sessions)
                    .SingleOrDefaultAsync(x => x.ScheduleId == putSession.ScheduleId);

                    if (schedule == null)
                    {
                        _logger.LogInformation("Failed to move session {SessionId}: schedule {ScheduleId} not found", putSession.SessionId, putSession.ScheduleId);
                        return BadRequest($"No schedule with id:{putSession.ScheduleId} found");
                    }
                    if (leagueId != schedule.LeagueId)
                    {
                        _logger.LogInformation("Failed to move session {SessionId}: schedule {ScheduleId} does not belong to league {LeagueName}",
                            putSession.SessionId, putSession.ScheduleId, leagueName);
                        return WrongLeague($"Schedule with id:{putSession.ScheduleId} does not belong to the specified league");
                    }

                    schedule.Sessions.Add(dbSession);
                }
            }

            // update parent session if changed
            if (dbSession.ParentSessionId != putSession.ParentSessionId)
            {
                _logger.LogInformation("Move session {SessionId} to parent session {ParentSessionId}", putSession.SessionId, putSession.ParentSessionId);
                if (putSession.ParentSessionId == null)
                {
                    await _dbContext.Entry(dbSession)
                        .Reference(x => x.ParentSession)
                        .LoadAsync();
                    dbSession.ParentSession.SubSessions.Remove(dbSession);
                }
                else
                {
                    var parentSession = await _dbContext.Sessions
                        .SingleOrDefaultAsync(x => x.SessionId == putSession.ParentSessionId);

                    if (parentSession == null)
                    {
                        _logger.LogInformation("Failed to move session {SessionId}: parent session {ParentSessionId} not found", putSession.SessionId, putSession.ParentSessionId);
                        return BadRequest($"No session with id:{putSession.ParentSessionId} found");
                    }
                    if (parentSession == dbSession)
                    {
                        _logger.LogInformation("Failed to move session {SessionId}: parent session is same as child session", putSession.SessionId, putSession.ParentSessionId);
                        return BadRequest($"Parent session is same as entry id:{putSession.ParentSessionId}");
                    }
                    if (parentSession.LeagueId != leagueId)
                    {
                        _logger.LogInformation("Failed to move session {SessionId}: parent session {ParentSessionId} does not belong to league {LeagueName}",
                            putSession.SessionId, putSession.ParentSessionId, leagueName);
                        return WrongLeague($"Session with id:{parentSession.SessionId} does not belong to the specified league");
                    }

                    parentSession.SubSessions.Add(dbSession);
                }
            }

            dbSession.Date = putSession.Date;
            dbSession.Duration = putSession.Duration;
            dbSession.Laps = putSession.Laps;
            dbSession.Name = putSession.Name;
            dbSession.PracticeAttached = putSession.PracticeAttached;
            dbSession.PracticeLength = putSession.PracticeLength;
            dbSession.QualyAttached = putSession.QualyAttached;
            dbSession.QualyLength = putSession.QualyLength;
            dbSession.RaceLength = putSession.RaceLength;
            dbSession.SessionTitle = putSession.SessionTitle;
            //dbSession.SessionType = putSession.SessionType;
            dbSession.SubSessionNr = putSession.SubSessionNr;
            dbSession.LastModifiedOn = DateTime.Now;
            dbSession.LastModifiedByUserId = currentUserID;
            dbSession.LastModifiedByUserName = User.Identity.Name;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Written session data on {LeagueName} for session {SessionId} by {UserName}", leagueName,
                dbSession.SessionId, User.Identity.Name);

            var getSession = await _dbContext.Sessions
                .Select(MapToSessionModelExpr)
                .SingleAsync(x => x.SessionId == dbSession.SessionId);

            _logger.LogInformation("Return session entry from {LeagueName} for session id {SessionId}", leagueName,
                getSession.SessionId);
            return Ok(getSession);
        }

        [HttpDelete]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [ParameterIgnore] long leagueId, [FromQuery] long id)
        {
            _logger.LogInformation("Request to delete Session {SessionId} from {LeagueName} by {Username}", id, leagueName, User.Identity.Name);

            var dbSession = await _dbContext.Sessions
                .Include(x => x.Schedule)
                    .ThenInclude(x => x.Sessions)
                .SingleOrDefaultAsync(x => x.SessionId == id && x.LeagueId == leagueId);

            if (dbSession == null)
            {
                _logger.LogInformation("Session {SessionId} on {LeagueName} not found", id, leagueName);
                return NotFound();
            }

            _dbContext.Sessions.Remove(dbSession);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Session {SessionId} deleted from {LeagueName} by {Username}", id, leagueName, User.Identity.Name);

            return Ok();
        }
    }
}
