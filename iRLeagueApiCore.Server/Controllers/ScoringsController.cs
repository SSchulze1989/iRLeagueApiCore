using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Scorings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class ScoringsController : LeagueApiController
    {
        private readonly ILogger<ScoringsController> _logger;
        private readonly IMediator mediator;

        public ScoringsController(ILogger<ScoringsController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
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
        public async Task<ActionResult<IEnumerable<GetScoringModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Get scorings from {LeagueName} by {UserName}", leagueName,
                    User.Identity.Name);
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
        public async Task<ActionResult<GetScoringModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Get scoring {ScoringId} from {LeagueName} by {UserName}", id, leagueName,
                    User.Identity.Name);
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
        /// Post a new scoring to an existing season
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="seasonId"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("/{leagueName}/Seasons/{seasonId:long}/Scorings")]
        [HttpPost]
        public async Task<ActionResult<GetScoringModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long seasonId, [FromBody] PostScoringModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding new scoring to {LeagueName} season {SeasonId} by {UserName}", leagueName,
                    seasonId, User.Identity.Name);
                var request = new PostScoringRequest(leagueId, seasonId, model);
                var getScoring = await mediator.Send(request, cancellationToken);
                _logger.LogInformation("Returning entry for scoring {ScoringId} from {LeagueName}", getScoring.Id, leagueName);
                return CreatedAtAction(nameof(Get), new { leagueName, id = getScoring.Id }, getScoring);
            }
            catch (ValidationException ex)
            {
                _logger.LogInformation("Bad request - errors: {ValidationErrors}", ex.Errors.Select(x => x.ErrorMessage));
                return ex.ToActionResult();
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogInformation("Season {SeasonId} not found in {LeagueName}", seasonId, leagueName);
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
        public async Task<ActionResult<GetScoringModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutScoringModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Put scoring {ScoringId} inside {LeagueName} by {UserName}", leagueName,
                    id, User.Identity.Name);
                var request = new PutScoringRequest(leagueId, id, model);
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
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromRoute] long id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Delete scoring {ScoringId} from {LeagueName} by {UserName}", leagueName,
                    id, User.Identity.Name);
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

        /// <summary>
        /// Add an existing session to the scoring
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="id"></param>
        /// <param name="sessionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("{id:long}/AddSession/{sessionId:long}")]
        [HttpPost]
        public async Task<ActionResult> AddSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromRoute] long sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Add session {SessionId} to scoring {ScoringId} in {LeagueName} by {UserName}",
                    sessionId, id, leagueName, User.Identity.Name);
                var request = new ScoringAddSessionRequest(leagueId, id, sessionId);
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
                _logger.LogInformation("Session {SessionId} in scoring {ScoringId} not found inside {LeagueName}", id, leagueName);
                return NotFound();
            }
        }

        /// <summary>
        /// Remove an existing session from the scoring 
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="id"></param>
        /// <param name="sessionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("{id:long}/RemoveSession/{sessionId:long}")]
        [HttpPost]
        public async Task<ActionResult> RemoveSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromRoute] long sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Remove session {SessionId} from scoring {ScoringId} in {LeagueName} by {UserName}",
                    sessionId, id, leagueName, User.Identity.Name);
                var request = new ScoringRemoveSessionRequest(leagueId, id, sessionId);
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
                _logger.LogInformation("Session {SessionId} in scoring {ScoringId} not found inside {LeagueName}", id, leagueName);
                return NotFound();
            }
        }

        /// <summary>
        /// Get all scorings from a season
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="seasonId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("/{leagueName}/Seasons/{seasonId}/Scorings")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetScoringModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromRoute] long seasonId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Get scorings from season {SeasonId in {LeagueName} by {UserName}",
                    seasonId, leagueName, User.Identity.Name);
                var request = new GetScoringsFromSeasonRequest(leagueId, seasonId);
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
                _logger.LogInformation("Season {SeasonId} not found inside {LeagueName}", seasonId, leagueName);
                return NotFound();
            }
        }
    }
}
