using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    public abstract class LeagueApiController<T> : Controller
    {
        protected ILogger<T> _logger;

        protected async Task<ActionResult<long>> CheckLeagueAsync(string leagueName, LeagueDbContext dbContext)
        {
            var leagueInfo = (await dbContext.Leagues
               .Select(x => new { x.LeagueId, x.Name })
               .SingleOrDefaultAsync(x => x.Name == leagueName));
            var leagueId = leagueInfo?.LeagueId ?? 0;

            if (leagueId == 0)
            {
                return NotFound("League not found");
            }

            if (HasLeagueRole(User, leagueName) == false)
            {
                _logger.LogInformation("Permission denied for {Username} to get entry of {GetType} on {LeagueName}",
                    User.Identity.Name, typeof(T), leagueName);
                return Forbid();
            }

            return Ok(leagueId);
        }

        protected ActionResult WrongLeague()
        {
            return StatusCode((int)HttpStatusCode.Forbidden);
        }

        protected ObjectResult WrongLeague(string message)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, new
            {
                Error = "Wrong league",
                Message = message
            });
        }

        protected bool HasLeagueRole(IPrincipal user, string leagueName)
        {
            var leagueUserRole = $"{leagueName.ToLower()}_{UserRoles.User}";
            var leagueAdminRole = $"{leagueName.ToLower()}_{UserRoles.Admin}";
            return user.IsInRole(leagueUserRole) || user.IsInRole(leagueAdminRole) || user.IsInRole(UserRoles.Admin);
        }

        protected bool HasLeagueRole(IPrincipal user, string leagueName, string roleName)
        {
            var leagueRole = $"{leagueName.ToLower()}_{roleName}";
            return user.IsInRole(leagueRole) || user.IsInRole(UserRoles.Admin);
        }
    }
}
