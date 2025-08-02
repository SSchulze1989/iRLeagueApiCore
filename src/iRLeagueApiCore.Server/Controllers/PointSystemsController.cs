using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public sealed class PointSystemsController : LeagueApiController<PointSystemsController>
{
    public PointSystemsController(ILogger<PointSystemsController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [Route("")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PointSystemModel>>> GetAll([FromRoute] string leagueName, CancellationToken cancellationToken)
    {
        var request = new GetPointSystemsFromLeagueRequest();
        var getPointSystems = await mediator.Send(request, cancellationToken);
        return Ok(getPointSystems);
    }

    [HttpGet]
    [Route("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<PointSystemModel>> Get([FromRoute] string leagueName, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = new GetPointSystemRequest(id);
        var getPointSystem = await mediator.Send(request, cancellationToken);
        return Ok(getPointSystem);
    }

    [HttpGet]
    [Route("/{leagueName}/Seasons/{seasonId:long}/PointSystems")]
    [AllowAnonymous]
    public async Task<ActionResult<PointSystemModel>> GetFromSeason([FromRoute] string leagueName, [FromRoute] long seasonId,
        CancellationToken cancellationToken)
    {
        var request = new GetPointSystemsFromSeasonRequest(seasonId);
        var getPointSystems = await mediator.Send(request, cancellationToken);
        return Ok(getPointSystems);
    }

    [HttpPost]
    [Route("/{leagueName}/ChampSeasons/{champSeasonId:long}/PointSystems")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<PointSystemModel>> Post([FromRoute] string leagueName, [FromRoute] long champSeasonId,
        [FromBody] PostPointSystemModel postPointSystem, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostPointSystemToChampSeasonRequest(champSeasonId, leagueUser, postPointSystem);
        var getPointSystem = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { leagueName, id = getPointSystem.PointSystemId }, getPointSystem);
    }

    [HttpPut]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<PointSystemModel>> Put([FromRoute] string leagueName, [FromRoute] long id,
        [FromBody] PutPointSystemModel putPointSystem, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PutPointSystemRequest(id, leagueUser, putPointSystem);
        var getPointSystem = await mediator.Send(request, cancellationToken);
        return Ok(getPointSystem);
    }

    [HttpDelete]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = new DeletePointSystemRequest(id);
        await mediator.Send(request, cancellationToken);
        return NoContent();
    }
}
