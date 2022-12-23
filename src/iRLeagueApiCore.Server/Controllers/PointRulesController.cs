﻿using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public sealed class PointRulesController : LeagueApiController<PointRulesController>
{
    public PointRulesController(ILogger<PointRulesController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [Route("{id:long}")]
    public async Task<ActionResult<PointRuleModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] pointRule {PointRuleId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                GetUsername());
        var request = new GetPointRuleRequest(leagueId, id);
        var getPointRule = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Returning entry for pointRule {PointRuleId} from {LeagueName}", getPointRule.PointRuleId, leagueName);
        return Ok(getPointRule);
    }

    [HttpPost]
    [Route("")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<PointRuleModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId, PostPointRuleModel postPointRule,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] new pointRule to {LeagueName} by {UserName}", "Post", leagueName,
            GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostPointRuleRequest(leagueId, leagueUser, postPointRule);
        var getPointRule = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return created entry for pointRule {PointRuleId} from {LeagueName}", getPointRule.PointRuleId, leagueName);
        return CreatedAtAction(nameof(Get), new { leagueName, id = getPointRule.PointRuleId }, getPointRule);
    }

    [HttpPut]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<PointRuleModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        [FromBody] PutPointRuleModel putPointRule, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] pointRule {PointRuleId} from {LeagueName} by {UserName}", "Put", id, leagueName,
            GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PutPointRuleRequest(leagueId, id, leagueUser, putPointRule);
        var getPointRule = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return entry for pointRule {PointRuleId} from {LeagueName}", getPointRule.PointRuleId, leagueName);
        return Ok(getPointRule);
    }

    [HttpDelete]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] pointRule {PointRuleId} from {LeagueName} by {UserName}", "Delete",
            id, leagueName,
            GetUsername());
        var request = new DeletePointRuleRequest(leagueId, id);
        await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Deleted pointRule {PointRuleId} from {LeagueName}", id, leagueName);
        return NoContent();
    }
}
