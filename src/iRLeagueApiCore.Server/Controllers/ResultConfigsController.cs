using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
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
    [ApiController]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [TypeFilter(typeof(DefaultExceptionFilterAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class ResultConfigsController : LeagueApiController
    {
        private readonly ILogger<ResultConfigsController> _logger;
        private readonly IMediator mediator;

        public ResultConfigsController(ILogger<ResultConfigsController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<ResultConfigModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all resultConfigs from {LeagueName} by {UserName}", "Get", leagueName, GetUsername());
            var request = new GetResultConfigsFromLeagueRequest(leagueId);
            var getResultConfigs = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Returning {Count} entries for resultConfigs in {LeagueName}", getResultConfigs.Count(), leagueName);
            return Ok(getResultConfigs);
        }

        [HttpGet]
        [Route("{id:long}")]
        public async Task<ActionResult<ResultConfigModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] resultConfig {ResultConfigId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                    GetUsername());
            var request = new GetResultConfigRequest(leagueId, id);
            var getResultConfig = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Returning entry for resultConfig {ResultConfigId} from {LeagueName}", getResultConfig.ResultConfigId, leagueName);
            return Ok(getResultConfig);
        }

        [HttpPost]
        [Route("")]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult<ResultConfigModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromBody] PostResultConfigModel postResultConfig, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] new resultConfig to {LeagueName} by {UserName}", "Post", leagueName, 
                GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostResultConfigRequest(leagueId, leagueUser, postResultConfig);
            var getResultConfig = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for resultConfig {ResultConfigId} from {LeagueName}", getResultConfig.ResultConfigId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getResultConfig.ResultConfigId }, getResultConfig);
        }

        [HttpPut]
        [Route("{id:long}")]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult<ResultConfigModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            [FromBody] PutResultConfigModel putResultConfig, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] resultConfig {ResultConfigId} from {LeagueName} by {UserName}", "Put", id, leagueName,
                GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutResultConfigRequest(leagueId, id, leagueUser, putResultConfig);
            var getResultConfig = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for resultConfig {ResultConfigId} from {LeagueName}", getResultConfig.ResultConfigId, leagueName);
            return Ok(getResultConfig);
        }

        [HttpDelete]
        [Route("{id:long}")]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] resultConfig {ResultConfigId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName,
                GetUsername());
            var request = new DeleteResultConfigRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted resultConfig {ResultConfigId} from {LeagueName}", id, leagueName);
            return NoContent();
        }
    }
}
