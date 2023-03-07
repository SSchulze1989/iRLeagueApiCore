﻿using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
[Route("{leagueName}/[controller]")]
public sealed class ScoringsController : LeagueApiController<ScoringsController>
{
    public ScoringsController(ILogger<ScoringsController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    /// <summary>
    /// Get all scorings from league
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScoringModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetScoringsRequest(leagueId);
            var getScorings = await mediator.Send(request, cancellationToken);
            return Ok(getScorings);
        }
        catch (ValidationException ex)
        {
            _logger.LogInformation("Bad request - errors: {ValidationErrors}", ex.Errors.Select(x => x.ErrorMessage));
            return ex.ToActionResult();
        }
        catch (ResourceNotFoundException)
        {
            _logger.LogInformation("Scorings not found in {LeagueName}", leagueName);
            return NotFound();
        }
    }

    /// <summary>
    /// Get scoring from league by Id
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("{id:long}")]
    [HttpGet]
    public async Task<ActionResult<ScoringModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetScoringRequest(leagueId, id);
            var getScoring = await mediator.Send(request, cancellationToken);
            return Ok(getScoring);
        }
        catch (ValidationException ex)
        {
            _logger.LogInformation("Bad request - errors: {ValidationErrors}", ex.Errors.Select(x => x.ErrorMessage));
            return ex.ToActionResult();
        }
        catch (ResourceNotFoundException)
        {
            _logger.LogInformation("Scoring {ScoringId} not found in {LeagueName}", id, leagueName);
            return NotFound();
        }
    }

    /// <summary>
    /// Update existing scoring with Id
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("{id:long}")]
    [HttpPut]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult<ScoringModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromRoute] long id, [FromBody] PutScoringModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutScoringRequest(leagueId, id, leagueUser, model);
            var getScoring = await mediator.Send(request, cancellationToken);
            return Ok(getScoring);
        }
        catch (ValidationException ex)
        {
            _logger.LogInformation("Bad request - errors: {ValidationErrors}", ex.Errors.Select(x => x.ErrorMessage));
            return ex.ToActionResult();
        }
        catch (ResourceNotFoundException)
        {
            _logger.LogInformation("Scoring {ScoringId} not found inside {LeagueName}", id, leagueName);
            return NotFound();
        }
    }

    /// <summary>
    /// Delete existing scoring with id
    /// </summary>
    /// <param name="leagueName"></param>
    /// <param name="leagueId"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("{id:long}")]
    [HttpDelete]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteScoringRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogInformation("Bad request - errors: {ValidationErrors}", ex.Errors.Select(x => x.ErrorMessage));
            return ex.ToActionResult();
        }
        catch (ResourceNotFoundException)
        {
            _logger.LogInformation("Scoring {ScoringId} not found inside {LeagueName}", id, leagueName);
            return NotFound();
        }
    }
}
