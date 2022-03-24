using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    public abstract class LeagueApiController : Controller
    {
        /// <summary>
        /// Returns a generic "something went wrong" error message in case the error
        /// should not be forwarded to the user
        /// </summary>
        /// <returns></returns>
        protected ActionResult SomethingWentWrong()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Response()
            {
                Status = "Error",
                Message = "Something went wrong. Please try again to see if the error persists"
            });
        }

        protected ActionResult OkMessage(string message)
        {
            return new OkObjectResult(new Response()
            {
                Status = "Success",
                Message = message
            });
        }

        protected ActionResult WrongLeague()
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        protected ObjectResult WrongLeague(string message)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new Response()
            {
                Status = "Wrong league",
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
