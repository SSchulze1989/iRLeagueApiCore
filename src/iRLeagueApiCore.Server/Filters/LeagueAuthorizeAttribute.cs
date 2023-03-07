using iRLeagueApiCore.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;
using System.Security.Principal;

namespace iRLeagueApiCore.Server.Filters;

/// <summary>
/// Authorization filter to manage access to league resources bases on user roles specific to each league
/// <para>The pattern for league roles is {leagueName}:{roleName} so for example an admin for testleague must be in the role: testleague:Admin</para>
/// <para><b>The decorated class or method must have {leagueName} as a route parameter.</b></para> 
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class LeagueAuthorizeAttribute : ActionFilterAttribute
{
    private readonly ILogger<LeagueAuthorizeAttribute> _logger;
    private readonly IDiagnosticContext diagnosticContext;

    public LeagueAuthorizeAttribute(ILogger<LeagueAuthorizeAttribute> logger, IDiagnosticContext diagnosticContext)
    {
        _logger = logger;
        this.diagnosticContext = diagnosticContext;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // infer league name from context
        if (context.RouteData.Values.TryGetValue("leagueName", out var leagueNameObject) == false)
        {
            _logger.LogError("Missing [leagueName] parameter in action route");
            throw new InvalidOperationException("Missing [leagueName] in action route");
        }
        var leagueName = (string)leagueNameObject!;

        // get user from httpcontext
        var user = context.HttpContext.User;
        var userName = (user.Identity != null && user.Identity.IsAuthenticated) ? user.Identity.Name : "Anonymous";

        // _logger.LogDebug("Authorizing request for {UserName} on {leagueName}", userName, leagueName);

        // check if specific league role required
        var requireLeagueRoleAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireLeagueRoleAttribute>();
        var allowAnonymousAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .FirstOrDefault();
        if (allowAnonymousAttribute != null || requireLeagueRoleAttribute.Any() == false)
        {
            // Allow public access
            await AccessGranted(context, next);
            return;
        }

        if (user == null || user.Identity == null || user.Identity.IsAuthenticated == false)
        {
            _logger.LogInformation("Permission denied for Anonymous user on {LeagueName}. League is not public", leagueName);
            context.Result = new UnauthorizedResult();
            return;
        }

        var requiredRoles = requireLeagueRoleAttribute
            .SelectMany(x => x.Roles)
            .ToList();
        if (requiredRoles.Count > 0)
        {
            var hasRole = requiredRoles
                .Any(x => HasLeagueRole(user, leagueName, x));

            if (hasRole == false)
            {
                _logger.LogInformation("Permission denied for {User} on {LeagueName}. User is not in any required role {Roles}",
                    user.Identity.Name, leagueName, requiredRoles);
                AccessDenied(context, next);
                return;
            }
        }
        else if (HasAnyLeagueRole(user, leagueName) == false)
        {
            _logger.LogInformation("Permission denied for {User} on {LeagueName}. User is not in any league role", user.Identity.Name, leagueName);
            AccessDenied(context, next);
            return;
        }

        await AccessGranted(context, next);
    }

    private static bool HasAnyLeagueRole(IPrincipal user, string leagueName)
    {
        foreach (var roleName in LeagueRoles.RolesAvailable)
        {
            var leagueRole = LeagueRoles.GetLeagueRoleName(leagueName, roleName);
            if (leagueRole != null && user.IsInRole(leagueRole))
            {
                return true;
            }
        }
        return user.IsInRole(UserRoles.Admin);
    }

    private static bool HasLeagueRole(IPrincipal user, string leagueName, string roleName)
    {
        var leagueRole = LeagueRoles.GetLeagueRoleName(leagueName, roleName);
        if (leagueRole == null)
        {
            return false;
        }
        return user.IsInRole(leagueRole) || user.IsInRole(UserRoles.Admin);
    }

    private void AccessDenied(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.Result = new ForbidResult();
    }

    private async Task AccessGranted(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await base.OnActionExecutionAsync(context, next);
    }
}
