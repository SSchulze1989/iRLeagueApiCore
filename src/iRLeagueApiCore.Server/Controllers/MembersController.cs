using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

/// <summary>
/// Endpoint for retrieving and managin member information
/// </summary>
[Authorize]
[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public sealed class MembersController : LeagueApiController<MembersController>
{
    public MembersController(ILogger<MembersController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MemberInfoModel>>> GetFromLeague([FromRoute] string leagueName, [FromFilter] long leagueId,
        CancellationToken cancellationToken)
    {
        var request = new GetMembersFromLeagueRequest(leagueId);
        var getMembers = await mediator.Send(request, cancellationToken);
        return Ok(getMembers);
    }

    [HttpGet]
    [Route("/{leagueName}/Events/{eventId:long}/[controller]")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MemberInfoModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long eventId,
        CancellationToken cancellationToken)
    {
        var request = new GetMembersFromEventRequest(leagueId, eventId);
        var getMembers = await mediator.Send(request, cancellationToken);
        return Ok(getMembers);
    }
}
