using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Rosters;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
[Route("{leagueName}/[controller]")]
public class RostersController : LeagueApiController<RostersController>
{
    public RostersController(ILogger<RostersController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [Route("")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RosterInfoModel>>> GetList([FromRoute] string leagueName, CancellationToken cancellationToken)
    {
        var request = new GetRosterListRequest();
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Route("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<RosterModel>> Get([FromRoute] string leagueName, [FromRoute] long id, CancellationToken cancellationToken)
    {
        var request = new GetRosterRequest(id);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<RosterModel>> Post([FromRoute] string leagueName, [FromBody] PostRosterModel model, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostRosterRequest(model, leagueUser);
        var result = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { leagueName, id = result.RosterId }, result);
    }

    [HttpPut]
    [Route("{id:long}")]
    public async Task<ActionResult<RosterModel>> Put([FromRoute] string leagueName, [FromRoute] long id, [FromBody] PutRosterModel model, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser (leagueName, User);
        var request = new PutRosterRequest(id, model, leagueUser);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [Route("{id:long}")]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromRoute] long id, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new DeleteRosterRequest(id, leagueUser);
        var resutl = await mediator.Send(request, cancellationToken);
        return NoContent();
    }
}
