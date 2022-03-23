﻿using iRLeagueApiCore.Server.Authentication;
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

        protected bool HasLeagueRole(IPrincipal user, string leagueName, string roleName)
        {
            var leagueRole = $"{leagueName.ToLower()}_{roleName}";
            return user.IsInRole(leagueRole) || user.IsInRole(UserRoles.Admin);
        }
    }
}
