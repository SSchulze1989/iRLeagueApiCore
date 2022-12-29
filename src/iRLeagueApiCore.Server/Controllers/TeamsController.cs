using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Teams;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[Authorize]
[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public sealed class TeamsController : LeagueApiController<TeamsController>
{
    public TeamsController(ILogger<TeamsController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<TeamModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] all Teams from {LeagueName} by {UserName}", "Get", leagueName, GetUsername());
        var request = new GetTeamsFromLeagueRequest(leagueId);
        var getTeams = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Returning {Count} entries for Teams in {LeagueName}", getTeams.Count(), leagueName);
        return Ok(getTeams);
    }

    [HttpGet]
    [Route("{id:long}")]
    public async Task<ActionResult<TeamModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] Team {TeamId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                GetUsername());
        var request = new GetTeamRequest(leagueId, id);
        var getTeam = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Returning entry for Team {TeamId} from {LeagueName}", getTeam.TeamId, leagueName);
        return Ok(getTeam);
    }

    [HttpPost]
    [Route("")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<TeamModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromBody] PostTeamModel postTeam, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] new Team to {LeagueName} by {UserName}", "Post", leagueName,
            GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostTeamRequest(leagueId, leagueUser, postTeam);
        var getTeam = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return created entry for Team {TeamId} from {LeagueName}", getTeam.TeamId, leagueName);
        return CreatedAtAction(nameof(Get), new { leagueName, id = getTeam.TeamId }, getTeam);
    }

    [HttpPut]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<TeamModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        [FromBody] PutTeamModel putTeam, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] Team {TeamId} from {LeagueName} by {UserName}", "Put", id, leagueName,
            GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PutTeamRequest(leagueId, id, leagueUser, putTeam);
        var getTeam = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return entry for Team {TeamId} from {LeagueName}", getTeam.TeamId, leagueName);
        return Ok(getTeam);
    }

    [HttpDelete]
    [Route("{id:long}")]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] Team {TeamId} from {LeagueName} by {UserName}", "Delete",
            id, leagueName,
            GetUsername());
        var request = new DeleteTeamRequest(leagueId, id);
        await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Deleted Team {TeamId} from {LeagueName}", id, leagueName);
        return NoContent();
    }
}
