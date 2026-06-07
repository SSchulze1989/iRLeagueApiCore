using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.DriverPoints;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[Authorize]
[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[Route("{leagueName}/[controller]")]
public sealed class DriverPointsController : LeagueApiController<DriverPointsController>
{
    public DriverPointsController(ILogger<DriverPointsController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{accountId:long}")]
    public async Task<ActionResult<DriverPointsAccountModel>> Get([FromRoute] string leagueName, [FromRoute] long accountId, CancellationToken cancellationToken)
    {
        var request = new GetDriverPointsAccountRequest(accountId);
        var account = await mediator.Send(request, cancellationToken);
        return Ok(account);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Member/{memberId:long}")]
    public async Task<ActionResult<IEnumerable<DriverPointsAccountModel>>> GetFromMember([FromRoute] string leagueName, [FromRoute] long memberId, CancellationToken cancellationToken)
    {
        var request = new GetDriverPointsForMemberRequest(memberId);
        var accounts = await mediator.Send(request, cancellationToken);
        return Ok(accounts);
    }

    [HttpPost]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
    public async Task<ActionResult<DriverPointsAccountModel>> Post([FromRoute] string leagueName, [FromBody] CreateDriverPointsAccountModel model, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostDriverPointsAccountRequest(leagueUser, model);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Route("{accountId:long}/Entry")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
    public async Task<ActionResult<DriverPointsEntryModel>> PostEntry([FromRoute] string leagueName, [FromRoute] long accountId, [FromBody] CreateDriverPointsEntryModel model, CancellationToken cancellationToken)
    {
        var request = new PostDriverPointsEntryRequest(accountId, model);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPatch]
    [Route("Entry/{entryId:long}/Archive")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
    public async Task<ActionResult> ArchiveEntry([FromRoute] string leagueName, [FromRoute] long entryId, CancellationToken cancellationToken)
    {
        var request = new ArchiveDriverPointsEntryRequest(entryId);
        await mediator.Send(request, cancellationToken);
        return Ok();
    }
}
