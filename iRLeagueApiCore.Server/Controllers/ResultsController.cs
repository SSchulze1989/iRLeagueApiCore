using iRLeagueApiCore.Communication.Models;
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
        /// <param name="sessionId"></param>
        /// <param name="scoringId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/{leagueName}/Session/{sessionId:long}/Scoring/{scoringId:long}/[controller]")]
        public async Task<ActionResult<GetResultModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long sessionId,
            [FromRoute] long scoringId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] result from session {SessionId} and scoring {ScoringId} in {LeagueName} by {UserName}", 
                "Get", sessionId, scoringId, leagueName, User.Identity.Name);
            var request = new GetResultRequest(leagueId, sessionId, scoringId);
            var getResult = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for result {SessionId},{ScoringId} from {LeagueName}", 
                getResult.SessionId, getResult.ScoringId, leagueName);
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
        [Route("/{leagueName}/Season/{seasonId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<GetResultModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId,
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
        /// <param name="sessionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/{leagueName}/Session/{sessionId:long}/[controller]")]
        public async Task<ActionResult<IEnumerable<GetResultModel>>> GetFromSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long sessionId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] all results from session {SessionId} in {LeagueName} by {UserName}",
                "Get", sessionId, leagueName, User.Identity.Name);
            var request = new GetResultsFromSessionRequest(leagueId, sessionId);
            var getResults = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for result from session {SessionId} in {LeagueName}",
                getResults.Count(), sessionId, leagueName);
            return Ok(getResults);
        }
    }
}
