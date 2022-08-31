using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using MediatR;
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
    [ApiController]
    [Route("/{leagueName}/[controller]")]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    public class ResultsController : LeagueApiController
    {
        private readonly ILogger<ResultsController> _logger;
        private readonly IMediator mediator;

        public ResultsController(ILogger<ResultsController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
        }

        /// <summary>
        /// Get single result from specific session and scoring
        /// </summary>
        /// <param name="leagueName"></param>
        /// <param name="leagueId"></param>
        /// <param name="eventId"></param>
        /// <param name="resultTabId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/{leagueName}/Events/{eventId:long}/ResultTabs/{resultTabId:long}/[controller]")]
        public async Task<ActionResult<EventResultTabModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long eventId,
            [FromRoute] long resultTabId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] result from session {EventId} and result tab {ResultTabId} in {LeagueName} by {UserName}", 
                "Get", eventId, resultTabId, leagueName, User.Identity.Name);
            var request = new GetResultRequest(leagueId, eventId, resultTabId);
            var getResult = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for result {EventId},{ResultTabId} from {LeagueName}", 
                getResult.EventId, getResult.ResultTabId, leagueName);
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
        [Route("/{leagueName}/Seasons/{seasonId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<ResultModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long seasonId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all results from season {SeasonId} in {LeagueName} by {UserName}",
                "Get", seasonId, leagueName, User.Identity.Name);
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
        [Route("/{leagueName}/Events/{eventId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<ResultModel>>> GetFromSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long eventId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all results from event {EventId} in {LeagueName} by {UserName}",
                "Get", eventId, leagueName, User.Identity.Name);
            var request = new GetResultsFromEventRequest(leagueId, eventId);
            var getResults = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for result from event {EventId} in {LeagueName}",
                getResults.Count(), eventId, leagueName);
            return Ok(getResults);
        }
    }
}
