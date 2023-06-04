using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
[Route("{leagueName}/[controller]")]
public sealed class PenaltiesController : LeagueApiController<PenaltiesController>
{
    public PenaltiesController(ILogger<PenaltiesController> logger, IMediator mediator) 
        : base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/{leagueName}/ScoredSessionResults/{id:long}/[controller]")]
    public async Task<ActionResult<IEnumerable<PenaltyModel>>> GetPenaltiesFromSessionResult([FromRoute] string leagueName, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = new GetPenaltiesFromSessionResult(id);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
