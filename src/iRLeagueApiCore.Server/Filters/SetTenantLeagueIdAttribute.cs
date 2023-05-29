using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iRLeagueApiCore.Server.Filters;

/// <summary>
/// Automatically insert the league id corresponding the league name in route parameters
/// <para>
/// Requires <b>"{leagueName}"</b> in Route
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SetTenantLeagueIdAttribute : ActionFilterAttribute
{
    private readonly LeagueDbContext _dbContext;
    private readonly RequestLeagueProvider leagueProvider;
    public SetTenantLeagueIdAttribute(LeagueDbContext dbContext, RequestLeagueProvider leagueProvider)
    {
        _dbContext = dbContext;
        this.leagueProvider = leagueProvider;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values.TryGetValue("leagueName", out var leagueNameObject) == false)
        {
            throw new InvalidOperationException("Missing {leagueName} in action route");
        }
        var leagueName = (string)leagueNameObject!;

        var league = await _dbContext.Leagues
            .Select(x => new { x.Id, x.Name })
            .SingleOrDefaultAsync(x => x.Name == leagueName);
        var leagueId = league?.Id ?? 0;

        if (leagueId == 0)
        {
            context.Result = new NotFoundObjectResult($"League {leagueName} does not exist");
            return;
        }

        context.ActionArguments.Add("leagueId", leagueId);
        leagueProvider.SetLeague(leagueId, leagueName);

        await base.OnActionExecutionAsync(context, next);
    }
}
