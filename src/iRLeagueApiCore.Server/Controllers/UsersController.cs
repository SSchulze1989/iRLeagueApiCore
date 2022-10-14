using iRLeagueApiCore.Common;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Extensions;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Users;
using iRLeagueApiCore.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
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
            var result = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for users from {LeagueName}", result.Count(), leagueName);
            return Ok(result);
        }

        [HttpGet]
        [TypeFilter(typeof(LeagueAuthorizeAttribute))]
        [RequireLeagueRole]
        [Route("/{leagueName}/[controller]/{id}")]
        public async Task<ActionResult<LeagueUserModel>> GetLeagueUser([FromRoute] string leagueName, [FromRoute] string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] user {UserId} from {LeagueName} by {Username}", "Get", id, leagueName, User.Identity?.Name);
            var request = new GetLeagueUserRequest(leagueName, id);
            var result = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for league user {UserId} from {LeagueName}", id, leagueName);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [Route("{id}")]
        public async Task<ActionResult<UserModel>> GetUser([FromRoute] string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] user {UserId} by {Username}", "Get", id, User.Identity?.Name);
            var currentUserId = User.GetUserId()!;
            object result;
            if (id == currentUserId)
            {
                // if the user requests its own info --> return private user data
                var request = new GetPrivateUserRequest(id);
                result = await mediator.Send(request,cancellationToken);
                _logger.LogInformation("Return private data for users {UserId}", id);
            }
            else
            {
                // else get public user data
                var request = new GetUserRequest(id);
                result = await mediator.Send(request, cancellationToken);
                _logger.LogInformation("Return public data for users {UserId}", id);
            }
            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        [Route("{id}")]
        public async Task<ActionResult<PrivateUserModel>> PutUser([FromRoute] string id, PutUserModel putUser, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] user data for user {UserId} by {Username}", "Put", id, User.Identity?.Name);
            var currentUserId = User.GetUserId();
            if (currentUserId != id)
            {
                // only the user (or manager admin) is allowed to change his own data
                return Forbid();
            }
            var request = new PutUserRequest(id, putUser);
            var result = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return updated data for user {UserId}", id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("Search")]
        public async Task<ActionResult<IEnumerable<UserModel>>> SearchByName([FromBody] SearchModel model, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] search for users with names {SearchKeys} by {Username}", "Post",
                model.SearchKeys, User.Identity?.Name);
            var request = new SearchUsersByNameRequest(model.SearchKeys.ToArray());
            var result = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for search", result.Count());
            return Ok(result);
        }

        [HttpPost]
        [TypeFilter(typeof(LeagueAuthorizeAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("/{leagueName}/[controller]/{id}/AddRole")]
        public async Task<ActionResult<LeagueUserModel>> AddUserRole([FromRoute] string leagueName, [FromRoute] string id, [FromBody] RoleModel role,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] Add role {RoleName} to user {RoleUserId} from {LeagueName} by {UserName}", "Post",
                role.RoleName, id, leagueName, User.Identity?.Name);
            var request = new AddLeagueRoleRequest(leagueName, id, role.RoleName);
            var result = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return league user data for {UserId} from {LeagueName}", id, leagueName);
            return Ok(result);
        }

        [HttpPost]
        [TypeFilter(typeof(LeagueAuthorizeAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin)]
        [Route("/{leagueName}/[controller]/{id}/RemoveRole")]
        public async Task<ActionResult<LeagueUserModel>> RemoveUserRole([FromRoute] string leagueName, [FromRoute] string id, [FromBody] RoleModel role,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] Remove role {RoleName} to user {RoleUserId} from {LeagueName} by {UserName}", "Post",
                role.RoleName, id, leagueName, User.Identity?.Name);
            var request = new RemoveLeagueRoleRequest(leagueName, id, role.RoleName);
            var result = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return league user data for {UserId} from {LeagueName}", id, leagueName);
            return Ok(result);
        }
    }
}
