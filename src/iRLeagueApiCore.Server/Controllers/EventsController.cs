using iRLeagueApiCore.Common;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Events;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    /// <summary>
    /// Endpoint for managing session entries
    /// </summary>
    [Authorize]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class EventsController : LeagueApiController<EventsController>
    {
        public EventsController(ILogger<EventsController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id:long}")]
        public async Task<ActionResult<EventModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] event {EventId} from {LeagueName} by {UserName}", "Get",
                id, leagueName, GetUsername());
            var request = new GetEventRequest(leagueId, id);
            var getEvent = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for event {EventId} from {LeagueName}", getEvent.Id, leagueName);
            return Ok(getEvent);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Schedules/{scheduleId:long}/Events")]
        public async Task<ActionResult<IEnumerable<EventModel>>> GetFromSchedule([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long scheduleId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all sessions in schedule {ScheduleId} from {LeagueName} by {UserName}", "Get",
                scheduleId, leagueName, GetUsername());
            var request = new GetEventsFromScheduleRequest(leagueId, scheduleId);
            var getEvents = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for events in schedule {ScheduleId} from {LeagueName}", 
                getEvents.Count(), scheduleId, leagueName);
            return Ok(getEvents);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Seasons/{seasonId:long}/Events")]
        public async Task<ActionResult<IEnumerable<EventModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromRoute] long seasonId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all sessions in season {SeasonId} from {LeagueName} by {UserName}", "Get", 
                seasonId, leagueName, GetUsername());
            var request = new GetEventsFromSeasonRequest(leagueId, seasonId);
            var getSessions = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for events in season {SeasonId} from {LeagueName}",
                getSessions.Count(), seasonId, leagueName);
            return Ok(getSessions);
        }

        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Schedules/{scheduleId:long}/Events")]
        public async Task<ActionResult<EventModel>> PostToSchedule([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long scheduleId, [FromBody] PostEventModel postEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new event to schedule {ScheduleId} in {LeagueName} by {UserName}", "Post",
                scheduleId, leagueName, GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostEventToScheduleRequest(leagueId, scheduleId, leagueUser, postEvent);
            var getEvent = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for event {EventId} from {LeagueName}", getEvent.Id, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getEvent.Id }, getEvent);
        }

        [HttpPut]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("{id:long}")]
        public async Task<ActionResult<EventModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutEventModel putEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] event {EventId} from {LeagueName} by {UserName}", "Put",
                id, leagueName, GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutEventRequest(leagueId, id, leagueUser, putEvent);
            var getEvent = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for event {EventId} from {LeagueName}", getEvent.Id , leagueName);
            return Ok(getEvent);
        }

        [HttpDelete]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("{id:long}")]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] event {EventId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName, GetUsername());
            var request = new DeleteEventRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted event {EventId} from {LeagueName}", id, leagueName);
            return NoContent();
        }
    }
}
