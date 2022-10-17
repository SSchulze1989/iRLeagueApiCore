using FluentValidation;
using iRLeagueApiCore.Common;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
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
                _logger.LogInformation("Get scorings from {LeagueName} by {UserName}", leagueName,
                    GetUsername());
                var request = new GetScoringsRequest(leagueId);
                var getScorings = await mediator.Send(request, cancellationToken);
                _logger.LogInformation("Returning {Count} entries for scorings from {LeagueName}", getScorings.Count(), leagueName);
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
                _logger.LogInformation("Get scoring {ScoringId} from {LeagueName} by {UserName}", id, leagueName,
                    GetUsername());
                var request = new GetScoringRequest(leagueId, id);
                var getScoring = await mediator.Send(request, cancellationToken);
                _logger.LogInformation("Returning entry for scoring {ScoringId} from {LeagueName}", id, leagueName);
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
                _logger.LogInformation("Put scoring {ScoringId} inside {LeagueName} by {UserName}", leagueName,
                    id, GetUsername());
                var leagueUser = new LeagueUser(leagueName, User);
                var request = new PutScoringRequest(leagueId, id, leagueUser, model);
                var getScoring = await mediator.Send(request, cancellationToken);
                _logger.LogInformation("Returning entry for scoring {ScoringId} from {LeagueName}", getScoring.Id, leagueName);
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
                _logger.LogInformation("Delete scoring {ScoringId} from {LeagueName} by {UserName}", leagueName,
                    id, GetUsername());
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
}
