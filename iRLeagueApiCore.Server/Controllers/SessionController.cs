﻿using iRLeagueApiCore.Communication.Enums;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
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
    [Route("{leagueName}/[controller]")]
    public class SessionController : LeagueApiController
    {
        private ILogger<SessionController> _logger;

        public SessionController(ILogger<SessionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSessionModel>>> Get([FromRoute] string leagueName, [FromQuery] long[] ids, [FromServices] LeagueDbContext dbContext)
        {
            if (HasLeagueRole(User, leagueName) == false)
            {
                return Forbid();
            }

            var leagueId = (await dbContext.Leagues
               .Select(x => new { x.LeagueId, x.Name })
               .SingleOrDefaultAsync(x => x.Name == leagueName))
               ?.LeagueId ?? 0;

            IQueryable<SessionEntity> dbSessions = dbContext.Sessions
                .Where(x => x.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbSessions = dbSessions.Where(x => ids.Contains(x.SessionId));
            }

            if (dbSessions.Count() == 0)
            {
                return NotFound();
            }

            var getSession = await dbSessions
                .Select(GetSessionModelFromDbExpression())
                .ToListAsync();

            return Ok(getSession);
        }
    
        [HttpPut]
        public async Task<ActionResult<GetSessionModel>> Put([FromRoute] string leagueName, [FromQuery] PutSessionModel putSession, [FromServices] LeagueDbContext dbContext)
        {
            if (HasLeagueRole(User, leagueName, UserRoles.Admin) == false)
            {
                return Forbid();
            }

            var leagueId = (await dbContext.Leagues
               .Select(x => new { x.LeagueId, x.Name })
               .SingleOrDefaultAsync(x => x.Name == leagueName))
               ?.LeagueId ?? 0;

            var dbSession = await dbContext.Sessions
                .SingleOrDefaultAsync(x => x.SessionId == putSession.SessionId);

            ClaimsPrincipal currentUser = User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbSession == null)
            {
                dbSession = new SessionEntity()
                {
                    LeagueId = leagueId,
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = currentUserID,
                    CreatedByUserName = User.Identity.Name
                };
                dbContext.Sessions.Add(dbSession);
            }
            else if (dbSession.LeagueId != leagueId)
            {
                return WrongLeague();
            }

            // update schedule if changed
            if (dbSession.ScheduleId != putSession.ScheduleId)
            {
                var schedule = await dbContext.Schedules
                    .Include(x => x.Sessions)
                    .SingleOrDefaultAsync(x => x.ScheduleId == putSession.ScheduleId);

                if (schedule == null)
                {
                    return BadRequest($"No schedule with id:{putSession.ScheduleId} found");
                }
                if (leagueId != schedule.LeagueId)
                {
                    return WrongLeague($"Schedule with id:{putSession.ScheduleId} does not belong to the specified league");
                }

                schedule.Sessions.Add(dbSession);
            }

            // update parent session if changed
            if (dbSession.ParentSessionId != putSession.ParentSessionId)
            {
                if (putSession.ParentSessionId == null)
                {
                    await dbContext.Entry(dbSession)
                        .Reference(x => x.ParentSession)
                        .LoadAsync();
                    dbSession.ParentSession.SubSessions.Remove(dbSession);
                }
                else
                {
                    var parentSession = await dbContext.Sessions
                        .SingleOrDefaultAsync(x => x.SessionId == putSession.ParentSessionId);

                    if (parentSession == null)
                    {
                        return BadRequest($"No session with id:{putSession.ParentSessionId} found");
                    }
                    if (parentSession == dbSession)
                    {
                        return BadRequest($"Parent session is same as entry id:{putSession.ParentSessionId}");
                    }
                    if (parentSession.LeagueId != leagueId)
                    {
                        return WrongLeague($"Session with id:{parentSession.SessionId} does not belong to the specified league");
                    }

                    parentSession.SubSessions.Add(dbSession);
                }
            }

            dbSession.Date = putSession.Date;
            dbSession.Duration = TimeSpan.FromSeconds(putSession.Duration);
            dbSession.Laps = putSession.Laps;
            dbSession.Name = putSession.Name;
            dbSession.PracticeAttached = putSession.PracticeAttached;
            dbSession.PracticeLength = putSession.PracticeLength != null ? TimeSpan.FromSeconds(putSession.PracticeLength.Value) : null;
            dbSession.QualyAttached = putSession.QualyAttached;
            dbSession.QualyLength = putSession.QualyLength != null ? TimeSpan.FromSeconds(putSession.QualyLength.Value) : null;
            dbSession.RaceLength = putSession.RaceLength != null ? TimeSpan.FromSeconds(putSession.RaceLength.Value) : null;
            dbSession.SessionTitle = putSession.SessionTitle;
            dbSession.SessionType = (int)putSession.SessionType;
            dbSession.SubSessionNr = putSession.SubSessionNr;

            await dbContext.SaveChangesAsync();

            var getSession = await dbContext.Sessions
                .Select(GetSessionModelFromDbExpression())
                .SingleAsync(x => x.SessionId == dbSession.SessionId);

            return Ok(getSession);
        }

        private Expression<Func<SessionEntity, GetSessionModel>> GetSessionModelFromDbExpression() =>
            x => new GetSessionModel()
            {
                SessionId = x.SessionId,
                ScheduleId = x.ScheduleId,
                LeagueId = x.LeagueId,
                PracticeAttached = x.PracticeAttached ?? false,
                QualyAttached = x.QualyAttached ?? false,
                PracticeLength = x.PracticeLength != null ? x.PracticeLength.Value.TotalSeconds : null,
                QualyLength = x.QualyLength != null ? x.QualyLength.Value.TotalSeconds : null,
                Date = x.Date,
                Duration = x.Duration.TotalSeconds,
                Laps = x.Laps ?? 0,
                RaceLength = x.RaceLength != null ? x.RaceLength.Value.TotalSeconds : null,
                Name = x.Name,
                SessionTitle = x.SessionTitle,
                //SessionType = (SessionTypeEnum)x.SessionType,
                SubSessionIds = x.SubSessions.Select(x => x.SessionId),
                ParentSessionId = x.ParentSessionId,
                SubSessionNr = x.SubSessionNr,
                TrackId = x.TrackId,
                HasResult = x.ResultEntity != null,
                CreatedOn = x.CreatedOn,
                CreatedByUserId = x.CreatedByUserId,
                CreatedByUserName = x.CreatedByUserName,
                LastModifiedOn = x.LastModifiedOn,
                LastModifiedByUserId = x.LastModifiedByUserId,
                LastModifiedByUserName = x.LastModifiedByUserName
            };
    }
}
