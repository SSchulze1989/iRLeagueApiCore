
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Triggers;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
[Route("{leagueName}/[controller]")]
public class TriggersController : LeagueApiController<TriggersController>
{
    public TriggersController(ILogger<TriggersController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TriggerModel>>> GetTriggers([FromRoute] string leagueName, CancellationToken cancellationToken = default)
    {
        var request = new GetLeagueTriggersRequest();
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TriggerModel>> PostTrigger([FromRoute] string leagueName, [FromBody] PostTriggerModel model,
        CancellationToken cancellationToken = default)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostTriggerRequest(leagueUser, model);
        var result = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetTrigger), new { leagueName, triggerId = result.TriggerId }, result);
    }

    [HttpPut("{triggerId:long}")]
    public async Task<ActionResult<TriggerModel>> PutTrigger([FromRoute] string leagueName, [FromRoute] long triggerId, [FromBody] PutTriggerModel model, 
        CancellationToken cancellationToken = default)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PutTriggerRequest(triggerId, leagueUser, model);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{triggerId:long}")]
    public async Task<ActionResult<TriggerModel>> GetTrigger([FromRoute] string leagueName, [FromRoute] long triggerId, CancellationToken cancellationToken = default)
    {
        var request = new GetTriggerRequest(triggerId);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{triggerId:long}")]
    public async Task<ActionResult> DeleteTrigger([FromRoute] string leagueName, [FromRoute] long triggerId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteTriggerRequest(triggerId);
        await mediator.Send(request, cancellationToken);
        return NoContent();
    }
}
