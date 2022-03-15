using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Filters
{
    public class LeagueAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly ILogger<LeagueAuthorizeAttribute> _logger;

        public LeagueAuthorizeAttribute(ILogger<LeagueAuthorizeAttribute> logger)
        {
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // infer league name from context
            if (context.RouteData.Values.TryGetValue("leagueName", out var leagueNameObject) == false)
            {
                _logger.LogError("Failed to authorize league: could not find {leagueName} in route values");
                throw new InvalidOperationException("Missing {leagueName} in action route");
            }
            if (context.ActionArguments.TryGetValue("dbContext", out var dbContextObject) == false)
            {
                _logger.LogError("Failed to authorize league: could not find 'dbContext' in action arguments");
                throw new InvalidOperationException("Missing 'dbContext' in action arguments");
            }
            var leagueName = (string)leagueNameObject;
            var dbContext = (LeagueDbContext)dbContextObject;

            // get user from httpcontext
            var user = context.HttpContext.User;
            var userName = user.Identity.IsAuthenticated ? user.Identity.Name : "Anonymous";

            _logger.LogInformation("Authorizing request for {UserName} on {leagueName}", userName, leagueName);

            var league = await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName);
            var leagueId = league?.LeagueId ?? 0;

            if (leagueId == 0)
            {
                _logger.LogInformation("League {LeagueName} not found in database", leagueName);
                context.Result = new NotFoundResult();
                return;
            }

            if (user == null || user.Identity.IsAuthenticated == false)
            {
                _logger.LogInformation("Permission denied for Anonymous user on {LeagueName}. League is not public", leagueName);
                context.Result = new ForbidResult();
                return;
            }

            if (HasLeagueRole(user, leagueName) == false)
            {
                _logger.LogInformation("Permission denied for {User} on {LeagueName}. User is not in league role", user.Identity.Name, leagueName);
                context.Result = new ForbidResult();
                return;
            }

            context.ActionArguments.Add("leagueId", leagueId);

            await base.OnActionExecutionAsync(context, next);
        }

        private bool HasLeagueRole(IPrincipal user, string leagueName)
        {
            var leagueUserRole = $"{leagueName.ToLower()}_{UserRoles.User}";
            var leagueAdminRole = $"{leagueName.ToLower()}_{UserRoles.Admin}";
            return user.IsInRole(leagueUserRole) || user.IsInRole(leagueAdminRole) || user.IsInRole(UserRoles.Admin);
        }

        private bool HasLeagueRole(IPrincipal user, string leagueName, string roleName)
        {
            var leagueRole = $"{leagueName.ToLower()}_{roleName}";
            return user.IsInRole(leagueRole) || user.IsInRole(UserRoles.Admin);
        }
    }
}
