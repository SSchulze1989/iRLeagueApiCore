using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Leagues;
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
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(DefaultExceptionFilterAttribute))]
    public class LeaguesController : LeagueApiController
    {
        private readonly ILogger<LeaguesController> _logger;
        private readonly IMediator mediator;

        public LeaguesController(ILogger<LeaguesController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<LeagueModel>>> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all leagues by {UserName}", "Get",
                User.Identity.Name);
            var request = new GetLeaguesRequest();
            var getLeagues = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} league entries", getLeagues.Count());
            return Ok(getLeagues);
        }

        [HttpGet]
        [Route("{id:long}")]
        public async Task<ActionResult<LeagueModel>> Get([FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] league {LeagueId} by {UserName}", 
                "Get", id, User.Identity.Name);
            var request = new GetLeagueRequest(id);
            var getLeague = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return league entry for id {LeagueId}", id);
            return Ok(getLeague);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("")]
        public async Task<ActionResult<LeagueModel>> Post([FromBody] PostLeagueModel postLeague, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new league by {UserName}",
                "Post", User.Identity.Name);
            var leagueUser = new LeagueUser(null, User);
            var request = new PostLeagueRequest(leagueUser, postLeague);
            var getLeague = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for {LeagueName}", getLeague.Name);
            return CreatedAtAction(nameof(Get), new { id = getLeague.Id }, getLeague);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{id}")]
        public async Task<ActionResult<LeagueModel>> Put([FromRoute] long id, [FromBody] PutLeagueModel putLeague, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] league data with {LeagueId} by {UserName}", id,
                "Put", User.Identity.Name);
            var leagueUser = new LeagueUser(null, User);
            var request = new PutLeagueRequest(id, leagueUser, putLeague);
            var getLeague = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return updated entry for {LeagueName}", getLeague.Name);
            return Ok(getLeague);
        }

        [HttpDelete]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{id}")]
        public async Task<ActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] league with id {LeagueId} by {UserName}", id,
                "Delete", User.Identity.Name);
            var request = new DeleteLeagueRequest(id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted league {LeagueId} by {UserName}", id,
                User.Identity.Name);
            return NoContent();
        }
    }
}
