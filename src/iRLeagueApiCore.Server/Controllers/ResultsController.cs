using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Client.ResultsParsing;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [Route("/{leagueName}/[controller]")]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole]
    public class ResultsController : LeagueApiController<ResultsController>
    {
        public ResultsController(ILogger<ResultsController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        /// <summary>
        /// Get single result from specific resultId
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="resultId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("{id:long}")]
        public async Task<ActionResult<EventResultModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long resultId, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] result {ResultId} in {LeagueName} by {UserName}", 
                "Get", resultId, leagueName, GetUsername());
            var request = new GetResultRequest(leagueId, resultId);
            var getResult = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for result {ResultId} from {LeagueName}", 
                getResult.ResultId, leagueName);
            return Ok(getResult);
        }

        /// <summary>
        /// Get all results from a season
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="seasonId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Seasons/{seasonId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<SeasonEventResultModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long seasonId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all results from season {SeasonId} in {LeagueName} by {UserName}",
                "Get", seasonId, leagueName, GetUsername());
            var request = new GetResultsFromSeasonRequest(leagueId, seasonId);
            var getResults = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for result from season {SeasonId} in {LeagueName}",
                getResults.Count(), seasonId, leagueName);
            return Ok(getResults);
        }

        /// <summary>
        /// Get all results from a session
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="eventId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Events/{eventId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<EventResultModel>>> GetFromSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long eventId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all results from event {EventId} in {LeagueName} by {UserName}",
                "Get", eventId, leagueName, GetUsername());
            var request = new GetResultsFromEventRequest(leagueId, eventId);
            var getResults = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for result from event {EventId} in {LeagueName}",
                getResults.Count(), eventId, leagueName);
            return Ok(getResults);
        }

        /// <summary>
        /// Upload a result json (must be exported from the iRacing GUI)
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="eventId"></param>
        /// <param name="result">complete json data exported from iRacing GUI</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Events/{eventId:long}/[controller]/Upload")]
        public async Task<ActionResult<bool>> UploadResult([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long eventId,
            [FromBody] ParseSimSessionResult result, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] raw results for event {EventId} in {LeagueName} by {UserName}", "Post", 
                eventId, leagueName, GetUsername());
            var request = new UploadResultRequest(leagueId, eventId, result);
            var success = await mediator.Send(request, cancellationToken);
            if (success)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest("Oops, something went wrong with the result upload!");
            }
        }

        [HttpPost]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("/{leagueName}/Events/{eventId:long}/[controller]/Calculate")]
        public async Task<ActionResult<bool>> TriggerResultCalculation([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long eventId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] trigger result calculation for event {EventId} in {LeagueName} by {UserName}", "Post",
                eventId, leagueName, GetUsername());
            var request = new TriggerResultCalculationCommand(leagueId, eventId);
            await mediator.Send(request, cancellationToken);
            return Ok(true);
        }
    }
}
