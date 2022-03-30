using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class ScoringsController : LeagueApiController
    {
        private readonly ILogger<ScoringsController> _logger;
        private readonly LeagueDbContext dbContext;
        private readonly IMediator mediator;

        public ScoringsController(ILogger<ScoringsController> logger, LeagueDbContext dbContext, IMediator mediator)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.mediator = mediator;
        }

        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [Route("{id:long}")]
        [HttpGet]
        public async Task<ActionResult<GetScoringModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Get scoring {ScoringId} from {LeagueName} by {UserName}", id, leagueName,
                    User.Identity.Name);
                var request = new GetScoringRequest(leagueId, id);
                var getScoring = await mediator.Send(request);
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

        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [Route("/{leagueName}/Seasons/{seasonId:long}/Scorings")]
        [HttpPost]
        public async Task<ActionResult<GetScoringModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long seasonId, [FromBody] PostScoringModel model)
        {
            try
            {
                _logger.LogInformation("Adding new scoring to {LeagueName} season {SeasonId} by {UserName}", leagueName,
                    seasonId, User.Identity.Name);
                var request = new PostScoringRequest(leagueId, seasonId, model);
                var getScoring = await mediator.Send(request);
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

        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [Route("{id:long}")]
        [HttpPut]
        public async Task<ActionResult<GetScoringModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutScoringModel model)
        {
            try
            {
                _logger.LogInformation("Put scoring {ScoringId} inside {LeagueName} by {UserName}", leagueName,
                    id, User.Identity.Name);
                var request = new PutScoringRequest(leagueId, id, model);
                var getScoring = await mediator.Send(request);
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

        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [Route("{id:long}")]
        [HttpDelete]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Delete scoring {ScoringId} from {LeagueName} by {UserName}", leagueName,
                    id, User.Identity.Name);
                var request = new DeleteScoringRequest(leagueId, id);
                await mediator.Send(request);
                return NoContent();
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogInformation("Scoring {ScoringId} not found inside {LeagueName}", id, leagueName);
                return NotFound();
            }
        }
    }
}
