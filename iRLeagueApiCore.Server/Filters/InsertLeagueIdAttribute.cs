﻿using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Filters
{
    /// <summary>
    /// Automatically insert the league id corresponding the league name in route parameters
    /// <para>
    /// Requires <b>{leagueName}</b> in Route and "<c><see cref="long"/> leagueId</c>" and "<c><see cref="LeagueDbContext"/> dbContext</c>" as parameters
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
    public class InsertLeagueIdAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.RouteData.Values.TryGetValue("leagueName", out var leagueNameObject) == false)
            {
                throw new InvalidOperationException("Missing {leagueName} in action route");
            }
            var leagueName = (string)leagueNameObject;
            if (context.ActionArguments.TryGetValue("dbContext", out var dbContextObject) == false)
            { 
                throw new InvalidOperationException("Missing 'dbContext' in action arguments");
            }
            var dbContext = (LeagueDbContext)dbContextObject;

            var league = await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName);
            var leagueId = league?.LeagueId ?? 0;

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
