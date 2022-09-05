using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
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
        [Route("{id:long}")]
        public async Task<ActionResult<ResultConfigModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] resultConfig {ResultConfigId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                    User.Identity.Name);
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
                User.Identity.Name);
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
            _logger.LogInformation("[{Method}] new resultConfig to {LeagueName} by {UserName}", "Put", leagueName,
                User.Identity.Name);
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutResultConfigRequest(leagueId, id, leagueUser, putResultConfig);
            var getResultConfig = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for resultConfig {ResultConfigId} from {LeagueName}", getResultConfig.ResultConfigId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getResultConfig.ResultConfigId }, getResultConfig);
        }

        [HttpDelete]
        [Route("{id:long}")]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] resultConfig {ResultConfigId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName,
                User.Identity.Name);
            var request = new DeleteResultConfigRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted resultConfig {ResultConfigId} from {LeagueName}", id, leagueName);
            return NoContent();
        }
    }
}
