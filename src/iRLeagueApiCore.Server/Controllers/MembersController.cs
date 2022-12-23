using iRLeagueApiCore.Common.Models.Members;
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
    [Route("/{leagueName}/Events/{eventId:long}/[controller]")]
    public async Task<ActionResult<IEnumerable<MemberInfoModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long eventId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] members in event {EventId} from {LeagueName} by {UserName}", "Get", eventId, leagueName,
            GetUsername());
        var request = new GetMembersFromEventRequest(leagueId, eventId);
        var getMembers = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Retur {Count} values for members in event {EventId} from {LeagueName}", getMembers.Count(), eventId, leagueName);
        return Ok(getMembers);
    }
}
