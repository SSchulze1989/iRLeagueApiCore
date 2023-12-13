using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
[Route("{leagueName}/[controller]")]
public class BonusesController : LeagueApiController<BonusesController>
{
    public BonusesController(ILogger<BonusesController> logger, IMediator mediator) 
        : base(logger, mediator)
    {
    }

    [HttpPost]
    [Route("/{leagueName}/ScoredSessionResults/{resultId:long}/Rows/{resultRowId:long}/[controller]")]
    public async Task<ActionResult<AddBonusModel>> PostBonusToResult([FromRoute] string leagueName, [FromRoute] long resultId, [FromRoute] long resultRowId,
        [FromBody] PostAddBonusModel postBonus, CancellationToken cancellationToken)
    {
        var request = new PostAddBonusRequest(resultRowId, postBonus);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Route("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<AddBonusModel>> GetBonus([FromRoute] string leagueName, [FromRoute] long id, CancellationToken cancellationToken)
    {
        var request = new GetAddBonusRequest(id);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Route("{id:long}")]
    public async Task<ActionResult<AddBonusModel>> PutBonus([FromRoute] string leagueName, [FromRoute] long id,
        [FromBody] PutAddBonusModel putBonus, CancellationToken cancellationToken)
    {
        var request = new PutAddBonusRequest(id, putBonus);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [Route("{id:long}")]
    public async Task<ActionResult> DeleteBonus([FromRoute] string leagueName, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = new DeleteAddBonusRequest(id);
        await mediator.Send(request, cancellationToken);
        return NoContent();
    }
}
