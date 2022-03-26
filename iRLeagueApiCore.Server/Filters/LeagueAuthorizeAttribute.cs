using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Filters
{
    /// <summary>
    /// Authorization filter to manage access to league resources bases on user roles specific to each league
    /// <para>The pattern for league roles is {leagueName}:{roleName} so for example an admin for testleague must be in the role: testleague:Admin</para>
    /// <para><b>The decorated class or method must have {leagueName} as a route parameter.</b></para> 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
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
            var leagueName = (string)leagueNameObject;

            // get user from httpcontext
            var user = context.HttpContext.User;
            var userName = user.Identity.IsAuthenticated ? user.Identity.Name : "Anonymous";

            _logger.LogInformation("Authorizing request for {UserName} on {leagueName}", userName, leagueName);

            if (user == null || user.Identity.IsAuthenticated == false)
            {
                _logger.LogInformation("Permission denied for Anonymous user on {LeagueName}. League is not public", leagueName);
                context.Result = new UnauthorizedResult();
                return;
            }

            // check if specific league role required
            var requireLeagueRoleAttribute = (RequireLeagueRoleAttribute)context.ActionDescriptor.EndpointMetadata
                .LastOrDefault(x => x.GetType() == typeof(RequireLeagueRoleAttribute));

            if (requireLeagueRoleAttribute?.Roles.Count() > 0)
            {
                var hasRole = requireLeagueRoleAttribute.Roles
                    .Any(x => HasLeagueRole(user, leagueName, x));

                if (hasRole == false)
                {
                    _logger.LogInformation("Permission denied for {User} on {LeagueName}. User is not in any required role {Roles}",
                        user.Identity.Name, leagueName, requireLeagueRoleAttribute.Roles);
                    context.Result = new ForbidResult();
                    return;
                }
            }
            else if (HasAnyLeagueRole(user, leagueName) == false)
            {
                _logger.LogInformation("Permission denied for {User} on {LeagueName}. User is not in any league role", user.Identity.Name, leagueName);
                context.Result = new ForbidResult();
                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }

        private bool HasAnyLeagueRole(IPrincipal user, string leagueName)
        {
            foreach (var roleName in LeagueRoles.RolesAvailable)
            {
                var leagueRole = LeagueRoles.GetLeagueRoleName(leagueName, roleName);
                if (user.IsInRole(leagueRole))
                {
                    return true;
                }
            }
            return user.IsInRole(UserRoles.Admin);
        }

        private bool HasLeagueRole(IPrincipal user, string leagueName, string roleName)
        {
            var leagueRole = LeagueRoles.GetLeagueRoleName(leagueName, roleName);
            return user.IsInRole(leagueRole) || user.IsInRole(UserRoles.Admin);
        }
    }
}
