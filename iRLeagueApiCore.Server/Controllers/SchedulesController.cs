using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SchedulesController : LeagueApiController
    {
        private readonly ILogger<SchedulesController> _logger;
        private readonly LeagueDbContext _dbContext;

        public SchedulesController(ILogger<SchedulesController> logger, LeagueDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetScheduleModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromQuery] long[] ids, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Get schedules from {LeagueName} for ids {ScheduleIds} by {UserName}", leagueName, ids,
                User.Identity.Name);

            IQueryable<ScheduleEntity> dbSchedules = _dbContext.Schedules
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

            var getSchedules = await dbSchedules
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
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Return {Count} schedule entries from {LeagueName} for ids {ScheduleIds}", getSchedules.Count(),
                leagueName, ids);
            return Ok(getSchedules);
        }

        [HttpPut]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("{id:long}")]
        public async Task<ActionResult<GetScheduleModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            [FromBody] PutScheduleModel putSchedule, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Put schedule data on {LeagueName} with id {ScheduleId} by {UserName}", leagueName,
                id, User.Identity.Name);

            var dbSchedule = await _dbContext.Schedules
                .SingleOrDefaultAsync(x => x.ScheduleId == id, cancellationToken);

            ClaimsPrincipal currentUser = User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            dbSchedule.Name = putSchedule.Name;
            dbSchedule.LastModifiedOn = DateTime.Now;
            dbSchedule.LastModifiedByUserId = currentUserID;
            dbSchedule.LastModifiedByUserName = User.Identity.Name;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Written schedule data on {LeagueName} for schedule {ScheduleId} by {UserName}", leagueName,
                dbSchedule.ScheduleId, User.Identity.Name);

            var getSchedule = await _dbContext.Schedules
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
                .SingleOrDefaultAsync(cancellationToken);

            _logger.LogInformation("Return schedule entry from {LeagueName} for schedule id {ScheduleId}", leagueName,
                getSchedule.ScheduleId);
            return Ok(getSchedule);
        }

        [HttpDelete]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromQuery] long id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting schedule {ScheduleId} from {LeagueName} by {UserName}", id, leagueName,
                User.Identity.Name);

            var dbSchedule = await _dbContext.Schedules
                .SingleOrDefaultAsync(x => x.ScheduleId == id, cancellationToken);

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

            _dbContext.Schedules.Remove(dbSchedule);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted schedule {ScheduleId} from {LeagueName} by {UserName}", id, leagueName,
                User.Identity.Name);
            return NoContent();
        }
    }
}
