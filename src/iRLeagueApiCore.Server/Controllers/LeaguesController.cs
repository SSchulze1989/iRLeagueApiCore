using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers
{
    [Route("[controller]")]
    public class LeaguesController : LeagueApiController<LeaguesController>
    {
        public LeaguesController(ILogger<LeaguesController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("")]
        public async Task<ActionResult<IEnumerable<LeagueModel>>> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all leagues by {UserName}", "Get",
                GetUsername());
            var request = new GetLeaguesRequest();
            var getLeagues = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} league entries", getLeagues.Count());
            return Ok(getLeagues);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id:long}")]
        public async Task<ActionResult<LeagueModel>> Get([FromRoute] long id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] league {LeagueId} by {UserName}",
                "Get", id, GetUsername());
            var request = new GetLeagueRequest(id);
            var getLeague = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return league entry for id {LeagueId}", id);
            return Ok(getLeague);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}")]
        public async Task<ActionResult<LeagueModel>> GetByName([FromRoute] string leagueName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] league {LeagueName} by {UserName}",
                "Get", leagueName, GetUsername());
            var request = new GetLeagueByNameRequest(leagueName);
            var getLeague = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return league entry for id {LeagueId}", getLeague.Id);
            return Ok(getLeague);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("")]
        public async Task<ActionResult<LeagueModel>> Post([FromBody] PostLeagueModel postLeague, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] new league by {UserName}",
                "Post", GetUsername());
            var leagueUser = new LeagueUser(string.Empty, User);
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
                "Put", GetUsername());
            var leagueUser = new LeagueUser(string.Empty, User);
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
                "Delete", GetUsername());
            var request = new DeleteLeagueRequest(id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted league {LeagueId} by {UserName}", id,
                GetUsername());
            return NoContent();
        }
    }
}
