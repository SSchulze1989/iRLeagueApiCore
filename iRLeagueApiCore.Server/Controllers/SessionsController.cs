using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Sessions;
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
    [ApiController]
    [Authorize]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SessionsController : LeagueApiController
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly IMediator mediator;

        public SessionsController(ILogger<SessionsController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<GetSessionModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all sessions from {LeagueName} by {UserName}", "Get", leagueName,
                User.Identity.Name);
            var request = new GetSessionsRequest(leagueId);
            var getSessions = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} session entries from {LeagueName}", getSessions.Count(),
                leagueName);
            return Ok(getSessions);
        }

        [HttpGet]
        [Route("{id:long}")]
        public async Task<ActionResult<GetSessionModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] session {SessionId} from {LeagueName} by {UserName}", "Get",
                id, leagueName, User.Identity.Name);
            var request = new GetSessionRequest(leagueId, id);
            var getSession = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for session {SessionId} from {LeagueName}", getSession.SessionId, leagueName);
            return Ok(getSession);
        }

        [HttpGet]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Schedules/{scheduleId:long}/Sessions")]
        public async Task<ActionResult<GetSessionModel>> PostToSchedule([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long scheduleId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all sessions in schedule {ScheduleId} from {LeagueName} by {UserName}", "Get", leagueName,
                scheduleId, User.Identity.Name);
            var request = new GetSessionsFromScheduleRequest(leagueId, scheduleId);
            var getSessions = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for sessions in schedule {ScheduleId} from {LeagueName}", 
                getSessions.Count(), scheduleId, leagueName);
            return Ok(getSessions);
        }

        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Schedules/{scheduleId:long}/Sessions")]
        public async Task<ActionResult<GetSessionModel>> PostToSchedule([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long scheduleId, [FromBody] PostSessionModel postSession, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new session to schedule {ScheduleId} in {LeagueName} by {UserName}", "Post", leagueName,
                scheduleId, User.Identity.Name);
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostSessionToScheduleRequest(leagueId, scheduleId, leagueUser, postSession);
            var getSession = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for session {SessionId} from {LeagueName}", getSession.SessionId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getSession.SessionId }, getSession);
        }

        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Sessions/{parentId:long}/SubSession")]
        public async Task<ActionResult<GetSessionModel>> PostToParentSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long parentId, [FromBody] PostSessionModel postSession, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new session to parent session {ParentSessionId} in {LeagueName} by {UserName}", "Post", leagueName,
                parentId, User.Identity.Name);
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostSessionToParentSessionRequest(leagueId, parentId, leagueUser, postSession);
            var getSession = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for session {SessionId} from {LeagueName}", getSession.SessionId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getSession.SessionId }, getSession);
        }

        [HttpPut]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("{id:long}")]
        public async Task<ActionResult<GetSessionModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutSessionModel putSession, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] session {SessionId} from {LeagueName} by {UserName}", "Put",
                leagueName, id, User.Identity.Name);
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutSessionRequest(leagueId, leagueUser, id, putSession);
            var getSession = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for session {SessionId} from {LeagueName}", leagueName,
                getSession.SessionId);
            return Ok(getSession);
        }

        [HttpDelete]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("{id:long}")]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] session {SessionId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName,
                User.Identity.Name);
            var request = new DeleteSessionRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted session {SessionId} from {LeagueName}", id, leagueName);
            return NoContent();
        }
    }
}
