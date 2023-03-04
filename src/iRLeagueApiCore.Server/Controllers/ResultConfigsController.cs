using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public sealed class ResultConfigsController : LeagueApiController<ResultConfigsController>
{
    public ResultConfigsController(ILogger<ResultConfigsController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [Route("")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ResultConfigModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId, CancellationToken cancellationToken)
    {
        var request = new GetResultConfigsFromLeagueRequest(leagueId);
        var getResultConfigs = await mediator.Send(request, cancellationToken);
        return Ok(getResultConfigs);
    }

    [HttpGet]
    [Route("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ResultConfigModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = new GetResultConfigRequest(leagueId, id);
        var getResultConfig = await mediator.Send(request, cancellationToken);
        return Ok(getResultConfig);
    }

    [HttpGet]
    [Route("/{leagueName}/Seasons/{seasonId:long}/ResultConfigs")]
    [AllowAnonymous]
    public async Task<ActionResult<ResultConfigModel>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long seasonId,
        CancellationToken cancellationToken)
    {
        var request = new GetResultConfigsFromSeasonRequest(leagueId, seasonId);
        var getResultConfigs = await mediator.Send(request, cancellationToken);
        return Ok(getResultConfigs);
    }

    [HttpPost]
    [Route("")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<ResultConfigModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromBody] PostResultConfigModel postResultConfig, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostResultConfigRequest(leagueId, leagueUser, postResultConfig);
        var getResultConfig = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { leagueName, id = getResultConfig.ResultConfigId }, getResultConfig);
    }

    [HttpPut]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<ResultConfigModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        [FromBody] PutResultConfigModel putResultConfig, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PutResultConfigRequest(leagueId, id, leagueUser, putResultConfig);
        var getResultConfig = await mediator.Send(request, cancellationToken);
        return Ok(getResultConfig);
    }

    [HttpDelete]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = new DeleteResultConfigRequest(leagueId, id);
        await mediator.Send(request, cancellationToken);
        return NoContent();
    }
}
