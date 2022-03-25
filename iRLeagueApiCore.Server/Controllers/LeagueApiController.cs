using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Models;
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
        protected ActionResult SomethingWentWrong()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Response()
            {
                Status = "Error",
                Message = "Something went wrong. Please try again to see if the error persists"
            });
        }

        /// <summary>
        /// Return a success request status with provided message
        /// </summary>
        protected ActionResult OkMessage(string message)
        {
            return new OkObjectResult(new Response()
            {
                Status = "Success",
                Message = message
            });
        }

        /// <summary>
        /// Return a success request status with provided result and message
        /// </summary>
        protected ActionResult OkMessage(string result, string message)
        {
            return new OkObjectResult(new ResultResponse()
            {
                Status = "Success",
                Result = result,
                Message = message
            });
        }

        /// <summary>
        /// Return a bad request status code with provided status and messag in body
        /// </summary>
        protected ActionResult BadRequestMessage(string result, string message)
        {
            return new BadRequestObjectResult(new ResultResponse()
            {
                Status = "Bad Request",
                Result = result,
                Message = message
            });
        }

        protected ActionResult WrongLeague()
        {
            return BadRequest();
        }

        protected ActionResult WrongLeague(string message)
        {
            return BadRequestMessage("Wrong league", message);
        }

        protected bool HasAnyLeagueRole(IPrincipal user, string leagueName)
        {
            foreach (var role in LeagueRoles.RolesAvailable)
            {
                var leagueRole = $"{leagueName.ToLower()}:{role}";
                if (user.IsInRole(leagueRole))
                {
                    return true;
                }
            }
            return user.IsInRole(UserRoles.Admin);
        }

        protected bool HasLeagueRole(IPrincipal user, string leagueName, string roleName)
        {
            var leagueRole = $"{leagueName.ToLower()}:{roleName}";
            return user.IsInRole(leagueRole) || user.IsInRole(UserRoles.Admin);
        }
    }
}
