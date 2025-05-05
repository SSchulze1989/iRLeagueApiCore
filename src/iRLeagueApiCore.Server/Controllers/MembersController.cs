using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Members;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

/// <summary>
/// Endpoint for retrieving and managin member information
/// </summary>
[Authorize]
[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(SetTenantLeagueIdAttribute))]
[Route("{leagueName}/[controller]")]
public sealed class MembersController : LeagueApiController<MembersController>
{
    public MembersController(ILogger<MembersController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{memberId:long}")]
    public async Task<ActionResult<MemberModel>> Get([FromRoute] string leagueName, [FromRoute] long memberId, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var includeProfile = IsIncludeProfile(leagueUser);
        var request = new GetMemberRequest(memberId, IncludeProfile: includeProfile);
        var getMember = await mediator.Send(request, cancellationToken);
        return Ok(getMember);
    }

    [HttpPut]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    [Route("{memberId:long}")]
    public async Task<ActionResult<MemberModel>> Put([FromRoute] string leagueName, [FromRoute] long memberId, [FromBody] PutMemberModel putMember, CancellationToken cancellationToken)
    {
        var request = new PutMemberRequest(memberId, putMember);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MemberInfoModel>>> GetFromLeague([FromRoute] string leagueName, CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var includeProfile = IsIncludeProfile(leagueUser);
        var request = new GetMembersFromLeagueRequest(IncludeProfile: includeProfile);
        var getMembers = await mediator.Send(request, cancellationToken);
        return Ok(getMembers);
    }

    [HttpGet]
    [Route("/{leagueName}/Events/{eventId:long}/[controller]")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MemberInfoModel>>> GetFromEvent([FromRoute] string leagueName, [FromRoute] long eventId,
        CancellationToken cancellationToken)
    {
        var leagueUser = new LeagueUser(leagueName, User);
        var includeProfile = IsIncludeProfile(leagueUser);
        var request = new GetMembersFromEventRequest(eventId, IncludeProfile: includeProfile);
        var getMembers = await mediator.Send(request, cancellationToken);
        return Ok(getMembers);
    }

    private static bool IsIncludeProfile(LeagueUser user)
    {
        return user.IsInRole(LeagueRoles.Admin, LeagueRoles.Organizer);
    }
}
