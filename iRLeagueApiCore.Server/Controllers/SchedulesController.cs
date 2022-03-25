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
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SchedulesController : LeagueApiController
    {
        private readonly ILogger<SchedulesController> _logger;

        public SchedulesController(ILogger<SchedulesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [InsertLeagueId]
        public async Task<ActionResult<IEnumerable<GetScheduleModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromQuery] long[] ids, [FromServices] LeagueDbContext dbContext)
        {
            _logger.LogInformation("Get schedules from {LeagueName} for ids {ScheduleIds} by {UserName}", leagueName, ids,
                User.Identity.Name);

            IQueryable<ScheduleEntity> dbSchedules = dbContext.Schedules
                .Where(x => x.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbSchedules = dbSchedules.Where(x => ids.Contains(x.ScheduleId));
            }

            if (dbSchedules.Count() == 0)
            {
                _logger.LogInformation("No schedules found in {LeagueName} for ids {ScheduleIds}", leagueName, ids);
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

            _logger.LogInformation("Return {Count} schedule entries from {LeagueName} for ids {ScheduleIds}", getSchedule.Count(), 
                leagueName, ids);
            return Ok(getSchedule);
        }

        [HttpPut]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [InsertLeagueId]
        public async Task<ActionResult<GetScheduleModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromBody] PutScheduleModel putSchedule, [FromServices] LeagueDbContext dbContext)
        {
            _logger.LogInformation("Put schedule data on {LeagueName} with id {ScheduleId} by {UserName}", leagueName,
                putSchedule.ScheduleId, User.Identity.Name);

            var dbSchedule = await dbContext.Schedules
                .SingleOrDefaultAsync(x => x.ScheduleId == putSchedule.ScheduleId);

            ClaimsPrincipal currentUser = User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbSchedule == null)
            {
                _logger.LogInformation("Create schedule {ScheduleName}", putSchedule.Name);
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
                _logger.LogInformation("Schedule with id {ScheduleId} belongs to another league");
                return BadRequestMessage("Schedule not found", $"No schedule with id {putSchedule.ScheduleId} could be found");
            }

            // update season if id changed
            if (putSchedule.SeasonId != dbSchedule.SeasonId)
            {
                _logger.LogInformation("Move schedule with id {ScheduleId} to season with id {SeasonId}", putSchedule.ScheduleId, putSchedule.SeasonId);
                var season = await dbContext.Seasons
                    .Include(x => x.Schedules)
                    .SingleOrDefaultAsync(x => x.SeasonId == putSchedule.SeasonId);

                if (season == null)
                {
                    _logger.LogInformation("Failed to move schedule {ScheduleId}: season {SeasonId} not found", putSchedule.ScheduleId, putSchedule.SeasonId);
                    return BadRequest($"No season with id:{putSchedule.SeasonId} found");
                }
                if (leagueId != season.LeagueId)
                {
                    _logger.LogInformation("Failed to move schedule {ScheduleId}: season {SeasonId} not does not belong to league {LeagueName}", 
                        putSchedule.ScheduleId, putSchedule.SeasonId, leagueName);
                    return WrongLeague($"Season with id:{putSchedule.SeasonId} does not belong to the specified league");
                }

                season.Schedules.Add(dbSchedule);
            }

            dbSchedule.Name = putSchedule.Name;
            dbSchedule.LastModifiedOn = DateTime.Now;
            dbSchedule.LastModifiedByUserId = currentUserID;
            dbSchedule.LastModifiedByUserName = User.Identity.Name;

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Written schedule data on {LeagueName} for schedule id {ScheduleId} by {UserName}", leagueName,
                dbSchedule.ScheduleId, User.Identity.Name);

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

            _logger.LogInformation("Return schedule entry from {LeagueName} for schedule id {ScheduleId}", leagueName,
                getSchedule.ScheduleId);
            return Ok(getSchedule);
        }

        [HttpDelete]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [InsertLeagueId]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromQuery] long id, [FromServices] LeagueDbContext dbContext)
        {
            _logger.LogInformation("Deleting schedule {ScheduleId} from {LeagueName} by {UserName}", id, leagueName,
                User.Identity.Name);

            var dbSchedule = await dbContext.Schedules
                .SingleOrDefaultAsync(x => x.ScheduleId == id);

            if (dbSchedule == null)
            {
                _logger.LogInformation("Not deleted: Schedule {ScheduleId} does not exist", id);
                return NoContent();
            }
            if (dbSchedule.LeagueId != leagueId)
            {
                _logger.LogInformation("Forbid to delete schedule {ScheduleId} because it does not belong to {LeagueName}", 
                    id, leagueName);
                return Forbid();
            }

            dbContext.Schedules.Remove(dbSchedule);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted schedule {ScheduleId} from {LeagueName} by {UserName}", id, leagueName,
                User.Identity.Name);
            return NoContent();
        }
    }
}
