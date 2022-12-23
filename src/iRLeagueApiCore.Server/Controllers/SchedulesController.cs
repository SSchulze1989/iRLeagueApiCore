using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Schedules;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers
{
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SchedulesController : LeagueApiController<SchedulesController>
    {
        public SchedulesController(ILogger<SchedulesController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<ScheduleModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all schedules from {LeagueName} by {UserName}", "Get", leagueName,
                GetUsername());
            var request = new GetSchedulesRequest(leagueId);
            var getSchedules = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} schedule entries from {LeagueName}", getSchedules.Count(),
                leagueName);
            return Ok(getSchedules);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id:long}")]
        public async Task<ActionResult<ScheduleModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] schedule {ScheduleId} from {LeagueName} by {UserName}", "Get",
                id, leagueName, GetUsername());
            var request = new GetScheduleRequest(leagueId, id);
            var getSchedule = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for schedule {ScheduleId} from {LeagueName}", getSchedule.ScheduleId, leagueName);
            return Ok(getSchedule);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Seasons/{seasonId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<ScheduleModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long seasonId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all schedules from season {SeasonId} in {LeagueName} by {UserName}",
                "Get", seasonId, leagueName, GetUsername());
            var request = new GetSchedulesFromSeasonRequest(leagueId, seasonId);
            var getSchedules = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for schedules from season {SeasonId} in {LeagueName}",
                getSchedules.Count(), seasonId, leagueName);
            return Ok(getSchedules);
        }

        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Seasons/{seasonId:long}/[controller]")]
        public async Task<ActionResult<ScheduleModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long seasonId, [FromBody] PostScheduleModel postSchedule, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new schedule to {LeagueName} by {UserName}", "Post", leagueName,
                GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostScheduleRequest(leagueId, seasonId, leagueUser, postSchedule);
            var getSchedule = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for schedule {ScheduleId} from {LeagueName}", getSchedule.ScheduleId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getSchedule.ScheduleId }, getSchedule);
        }

        [HttpPut]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("{id:long}")]
        public async Task<ActionResult<ScheduleModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutScheduleModel putSchedule, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] schedule {ScheduleId} from {LeagueName} by {UserName}", "Put",
                leagueName, id, GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutScheduleRequest(leagueId, leagueUser, id, putSchedule);
            var getSchedule = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for schedule {ScheduleId} from {LeagueName}", leagueName,
                getSchedule.ScheduleId);
            return Ok(getSchedule);
        }

        [HttpDelete]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("{id:long}")]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] schedule {ScheduleId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName,
                GetUsername());
            var request = new DeleteScheduleRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted schedule {ScheduleId} from {LeagueName}", id, leagueName);
            return NoContent();
        }
    }
}
