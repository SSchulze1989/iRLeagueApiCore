using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
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
    [TypeFilter(typeof(DefaultExceptionFilterAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SeasonsController : LeagueApiController
    {
        private readonly ILogger<SeasonsController> _logger;
        private readonly IMediator mediator;

        public SeasonsController(ILogger<SeasonsController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<GetSeasonModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all seasons from {LeagueName} by {UserName}", "Get", leagueName,
                User.Identity.Name);
            var request = new GetSeasonsRequest(leagueId);
            var getSeasons = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} season entries from {LeagueName}", getSeasons.Count(),
                leagueName);
            return Ok(getSeasons);
        }

        [HttpGet]
        [Route("{id:long}")]
        public async Task<ActionResult<GetSeasonModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] season {SeasonId} from {LeagueName} by {UserName}", "Get",
                id, leagueName, User.Identity.Name);
            var request = new GetSeasonRequest(leagueId, id);
            var getSeason = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for season {SeasonId} from {LeagueName}", getSeason.SeasonId, leagueName);
            return Ok(getSeason);
        }

        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("")]
        public async Task<ActionResult<GetSeasonModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromBody] PostSeasonModel postSeason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new season to {LeagueName} by {UserName}", "Post", leagueName, 
                User.Identity.Name);
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostSeasonRequest(leagueId, leagueUser, postSeason);
            var getSeason = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for season {SeasonId} from {LeagueName}", getSeason.SeasonId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getSeason.SeasonId }, getSeason);
        }

        [HttpPut]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("{id:long}")]
        public async Task<ActionResult<GetSeasonModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutSeasonModel putSeason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] season {SeasonId} from {LeagueName} by {UserName}", "Put",
                leagueName, id, User.Identity.Name);
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutSeasonRequest(leagueId, leagueUser, id, putSeason);
            var getSeason = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for season {SeasonId} from {LeagueName}", leagueName,
                getSeason.SeasonId);
            return Ok(getSeason);
        }

        [HttpDelete]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("{id:long}")]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] season {SeasonId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName,
                User.Identity.Name);
            var request = new DeleteSeasonRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted season {SeasonId} from {LeagueName}", id, leagueName,
                User.Identity.Name);
            return NoContent();
        }
    }
}
