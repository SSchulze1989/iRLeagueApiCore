using iRLeagueApiCore.Common.Models.Tracks;
using iRLeagueApiCore.Server.Handlers.Tracks;
using iRLeagueApiCore.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [Route("[controller]")]
    public class TracksController : LeagueApiController<TracksController>
    {
        public TracksController(ILogger<TracksController> logger, IMediator mediator) : 
            base(logger, mediator)
        {
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<TrackGroupModel>>> Get(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] Track list by {UserName}", "Get", GetUsername());
            var request = new GetTracksRequest();
            var getTracks = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Returning {Count} entries for Tracks", getTracks.Count());
            return Ok(getTracks);
        }

        [HttpPost("UpdateTracks")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ImportTracks([FromBody] IracingAuthModel authData, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] Command to import tracks from iracing api", "Post");
            var request = new ImportTracksCommand(authData);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Track update succesfull");
            return Ok();
        }
    }
}
