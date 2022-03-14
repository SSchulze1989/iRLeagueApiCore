using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Principal;

namespace iRLeagueApiCore.Server.Controllers
{
    public abstract class LeagueApiController : Controller
    {
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
    }
}
