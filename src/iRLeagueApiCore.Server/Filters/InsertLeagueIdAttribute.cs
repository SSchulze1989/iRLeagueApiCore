using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Filters
{
    /// <summary>
    /// Automatically insert the league id corresponding the league name in route parameters
    /// <para>
    /// Requires <b>{leagueName}</b> in Route and "<c><see cref="long"/> leagueId</c>" as parameter
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// [InsertLeague]
    /// [Route("{leagueName}/action")]
    /// public IActionResult Action([FromRoute] string leagueName, long leagueId, [FromServices] LeagueDbContext dbContext)
    /// {
    ///     ...
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class InsertLeagueIdAttribute : ActionFilterAttribute
    {
        private readonly LeagueDbContext _dbContext;
        public InsertLeagueIdAttribute(LeagueDbContext dbContext)
        {
            _dbContext = dbContext;
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

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
