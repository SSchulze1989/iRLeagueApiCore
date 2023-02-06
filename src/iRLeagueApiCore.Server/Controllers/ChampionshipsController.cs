﻿using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Championships;
using iRLeagueApiCore.Server.Handlers.Events;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[Authorize]
[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public class ChampionshipsController : LeagueApiController<ChampionshipsController>
{
    public ChampionshipsController(ILogger<ChampionshipsController> logger, IMediator mediator) : 
        base(logger, mediator)
    {
    }

    /// <summary>
    /// Get single championship by id
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("{id:long}")]
    public async Task<ActionResult<ChampionshipModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Method}] championship {ChampionshipId} from {LeagueName} by {UserName}", "Get",
            id, leagueName, GetUsername());
        var request = new GetChampionshipRequest(leagueId, id);
        var getChampionship = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return entry for championship {ChampionshipId} from {LeagueName}", getChampionship.ChampionshipId, leagueName);
        return Ok(getChampionship);
    }

    /// <summary>
    /// Get all championships from a league
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("")]
    public async Task<ActionResult<IEnumerable<ChampionshipModel>>> GetFromLeague([FromRoute] string leagueName, [FromFilter] long leagueId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Method}] all championships from {LeagueName} by {UserName}", "Get",
            leagueName, GetUsername());
        var request = new GetChampionshipsFromLeagueRequest(leagueId);
        var getChampionships = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return {Count} entries for championships from {LeagueName}", getChampionships.Count(), leagueName);
        return Ok(getChampionships);
    }

    /// <summary>
    /// Post a new championship
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="postChampionship"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    public async Task<ActionResult<ChampionshipModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId, [FromBody] PostChampionshipModel postChampionship,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Method}] new championship to {LeagueName} by {UserName}", "Post",
            leagueName, GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PostChampionshipRequest(leagueId, leagueUser, postChampionship);
        var getChampionship = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return created entry for championshipe {ChampionshipId} from {LeagueName}", getChampionship.ChampionshipId, leagueName);
        return CreatedAtAction(nameof(Get), new { leagueName, id = getChampionship.ChampionshipId }, getChampionship);
    }

    /// <summary>
    /// Update a single championship
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="id"></param>
    /// <param name="putChampionship"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    [Route("{id:long}")]
    public async Task<ActionResult<ChampionshipModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
    [FromRoute] long id, [FromBody] PutChampionshipModel putChampionship, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Method}] championship {ChampionshipId} from {LeagueName} by {UserName}", "Put",
            id, leagueName, GetUsername());
        var leagueUser = new LeagueUser(leagueName, User);
        var request = new PutChampionshipRequest(leagueId, id, leagueUser, putChampionship);
        var getChampionship = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return entry for championship {ChampionshipId} from {LeagueName}", getChampionship.ChampionshipId, leagueName);
        return Ok(getChampionship);
    }

    /// <summary>
    /// Delete a championship permanently
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [RequireLeagueRole(LeagueRoles.Admin)]
    [Route("{id:long}")]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Method}] championship {ChampionshipId} from {LeagueName} by {UserName}", "Delete",
            id, leagueName, GetUsername());
        var request = new DeleteChampionshipRequest(leagueId, id);
        await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Deleted championship {ChampionshipId} from {LeagueName}", id, leagueName);
        return NoContent();
    }
}
