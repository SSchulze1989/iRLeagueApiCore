using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[Route("[controller]")]
public sealed class LeaguesController : LeagueApiController<LeaguesController>
{
    public LeaguesController(ILogger<LeaguesController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("")]
    public async Task<ActionResult<IEnumerable<LeagueModel>>> GetAll(CancellationToken cancellationToken = default)
    {
        var request = new GetLeaguesRequest();
        var getLeagues = await mediator.Send(request, cancellationToken);
        return Ok(getLeagues);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{leagueId:long}")]
    public async Task<ActionResult<LeagueModel>> Get([FromRoute] long leagueId, CancellationToken cancellationToken = default)
    {
        var request = new GetLeagueRequest(leagueId);
        var getLeague = await mediator.Send(request, cancellationToken);
        return Ok(getLeague);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/{leagueName}")]
    public async Task<ActionResult<LeagueModel>> GetByName([FromRoute] string leagueName, CancellationToken cancellationToken = default)
    {
        var request = new GetLeagueByNameRequest(leagueName);
        var getLeague = await mediator.Send(request, cancellationToken);
        return Ok(getLeague);
    }

    [HttpPost]
    [Route("")]
    [Authorize(Roles = UserRoles.User)]
    public async Task<ActionResult<LeagueModel>> Post([FromBody] PostLeagueModel postLeague, CancellationToken cancellationToken = default)
    {
        var leagueUser = new LeagueUser(string.Empty, User);
        var request = new PostLeagueRequest(leagueUser, postLeague);
        var getLeague = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = getLeague.Id }, getLeague);
    }

    [HttpPut]
    [Route("{leagueId}")]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<LeagueModel>> Put([FromRoute] long leagueId, [FromBody] PutLeagueModel putLeague, CancellationToken cancellationToken = default)
    {
        var leagueUser = new LeagueUser(string.Empty, User);
        var request = new PutLeagueRequest(leagueId, leagueUser, putLeague);
        var getLeague = await mediator.Send(request, cancellationToken);
        return Ok(getLeague);
    }

    [HttpDelete]
    [Route("{leagueId}")]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult> Delete([FromRoute] long leagueId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteLeagueRequest(leagueId);
        await mediator.Send(request, cancellationToken);
        return NoContent();
    }
}
