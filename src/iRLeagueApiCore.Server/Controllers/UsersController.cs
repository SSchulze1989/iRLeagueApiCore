using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Users;
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
    [Route("[controller]")]
    public sealed class UsersController : LeagueApiController<UsersController>
    {
        public UsersController(ILogger<UsersController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        [TypeFilter(typeof(LeagueAuthorizeAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("/{leagueName}/[controller]")]
        public async Task<ActionResult<IEnumerable<AdminUserModel>>> GetAllLeagueUsers([FromRoute] string leagueName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all users from {LeagueName} by {Username}", "Get", leagueName, User.Identity?.Name);
            var request = new GetUserListRequest(leagueName);
            var result = await mediator.Send(request);
            _logger.LogInformation("Return {Count} entries for users from {LeagueName}", result.Count(), leagueName);
            return Ok(result);
        }
    }
}
