using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[Route("{leagueName}/[controller]")]
public class ProtestsController : LeagueApiController<ProtestsController>
{
    public ProtestsController(ILogger<ProtestsController> logger, IMediator mediator) : 
        base(logger, mediator)
    {
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("/{leagueName}/Sessions/{sessionId:long}/[controller]")]
    public async Task<ActionResult<ProtestModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long sessionId,
        PostProtestModel postReview, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] protest to session {SessionId} from {LeagueName} by {UserName}", "Post",
            sessionId, leagueName, GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostProtestToSessionRequest(leagueId, sessionId, leagueUser, postReview);
        var getProtest = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return created entry for protest {ProtestId} from {LeagueName}", getProtest.ProtestId, leagueName);
        return Created(string.Empty, getProtest);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/{leagueName}/Events/{eventId:long}/Protests")]
    public async Task<ActionResult<IEnumerable<ProtestModel>>> GetFromEvent([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromRoute] long eventId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] all protests on event {EventId} from {LeagueName} by {UserName}", "Get",
            eventId, leagueName, GetUsername());
        var request = new GetProtestsFromEventRequest(leagueId, eventId);
        var getProtests = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return {Count} entries for protests on event {EventId} from {LeagueName}",
            getProtests.Count(), eventId, leagueName);
        return Ok(getProtests);
    }

    [HttpDelete]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
    [Route("{id:long}")]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] protest {ProtestId} from {LeagueName} by {UserName}", "Delete",
            id, leagueName,
            GetUsername());
        var request = new DeleteProtestRequest(leagueId, id);
        await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Deleted protest {ProtestId} from {LeagueName}", id, leagueName);
        return NoContent();
    }
}
