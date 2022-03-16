using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("{leagueName}/[controller]")]
    [Authorize]
    public class ScheduleController : LeagueApiController<ScheduleController>
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetScheduleModel>>> Get([FromRoute] string leagueName, [FromQuery] long[] ids, [FromServices] LeagueDbContext dbContext)
        {
            if (HasLeagueRole(User, leagueName) == false)
            {
                return Forbid();
            }

            var leagueId = (await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName))
                ?.LeagueId ?? 0;

            IQueryable<ScheduleEntity> dbSchedules = dbContext.Schedules
                .Where(x => x.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbSchedules = dbSchedules.Where(x => ids.Contains(x.ScheduleId));
            }

            if (dbSchedules.Count() == 0)
            {
                return NotFound();
            }

            var getSchedule = await dbSchedules
                .Select(x => new GetScheduleModel()
                {
                    ScheduleId = x.ScheduleId,
                    SeasonId = x.SeasonId,
                    Name = x.Name,
                    SessionIds = x.Sessions.Select(x => x.SessionId),
                    CreatedOn = x.CreatedOn,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName
                })
                .ToListAsync();

            return Ok(getSchedule);
        }

        [HttpPut]
        public async Task<ActionResult<GetScheduleModel>> Put([FromRoute] string leagueName, [FromBody] PutScheduleModel putSchedule, [FromServices] LeagueDbContext dbContext)
        {
            if (HasLeagueRole(User, leagueName) == false)
            {
                return Forbid();
            }

            var leagueId = (await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName))
                ?.LeagueId ?? 0;

            var dbSchedule = await dbContext.Schedules
                .SingleOrDefaultAsync(x => x.ScheduleId == putSchedule.ScheduleId);

            ClaimsPrincipal currentUser = User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbSchedule == null)
            {
                dbSchedule = new ScheduleEntity()
                {
                    LeagueId = leagueId,
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = currentUserID,
                    CreatedByUserName = User.Identity.Name
                };
                dbContext.Schedules.Add(dbSchedule);
            }
            else if (dbSchedule.LeagueId != leagueId)
            {
                return WrongLeague();
            }

            // update season if id changed
            if (putSchedule.SeasonId != dbSchedule.SeasonId)
            {
                var season = await dbContext.Seasons
                    .Include(x => x.Schedules)
                    .SingleOrDefaultAsync(x => x.SeasonId == putSchedule.SeasonId);

                if (season == null)
                {
                    return BadRequest($"No season with id:{putSchedule.SeasonId} found");
                }
                if (leagueId != season.LeagueId)
                {
                    return WrongLeague($"Season with id:{putSchedule.SeasonId} does not belong to the specified league");
                }

                season.Schedules.Add(dbSchedule);
            }

            dbSchedule.Name = putSchedule.Name;
            dbSchedule.LastModifiedOn = DateTime.Now;
            dbSchedule.LastModifiedByUserId = currentUserID;
            dbSchedule.LastModifiedByUserName = User.Identity.Name;

            await dbContext.SaveChangesAsync();

            var getSchedule = await dbContext.Schedules
                .Select(x => new GetScheduleModel()
                {
                    ScheduleId = x.ScheduleId,
                    SeasonId = x.SeasonId,
                    LeagueId = x.LeagueId,
                    Name = x.Name,
                    SessionIds = x.Sessions.Select(x => x.SessionId),
                    CreatedOn = x.CreatedOn,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName
                })
                .Where(x => x.ScheduleId == dbSchedule.ScheduleId)
                .SingleOrDefaultAsync();

            return Ok(getSchedule);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromQuery] long id, [FromServices] LeagueDbContext dbContext)
        {
            if (HasLeagueRole(User, leagueName) == false)
            {
                return Forbid();
            }

            var leagueId = (await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName))
                ?.LeagueId ?? 0;

            var dbSchedule = await dbContext.Schedules
                .SingleOrDefaultAsync(x => x.ScheduleId == id);

            if (dbSchedule == null)
            {
                return BadRequest($"Schedule with id:{id} does not exist");
            }
            else if (dbSchedule.LeagueId != leagueId)
            {
                return Forbid();
            }

            dbContext.Schedules.Remove(dbSchedule);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
