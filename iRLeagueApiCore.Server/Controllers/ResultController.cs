using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Filters;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("/{leagueName}/Result")]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    public class ResultController : LeagueApiController
    {
        private readonly ILogger<ResultController> _logger;

        public ResultController(ILogger<ResultController> logger)
        {
            _logger = logger;
        }

        private static Expression<Func<ScoredResultEntity, GetResultModel>> GetResultModelFromDbExpression => x => new GetResultModel()
        {
            SessionId = x.Result.ResultId,
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetResultModel>>> Get([FromRoute] string leagueName, [ParameterIgnore] long leagueId, 
            [FromQuery] long[] ids, [FromServices] LeagueDbContext dbContext)
        {
            _logger.LogInformation("Request: Get Results from {LeagueName} for ids {Ids} by {Username}", leagueName, ids,
                User.Identity.Name);

            IQueryable<ScoredResultEntity> dbResults = dbContext.ScoredResults
                .Where(x => x.Result.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbResults = dbResults
                    .Where(x => ids.Contains(x.ResultId));
            }

            if (await dbResults.CountAsync() == 0)
            {
                _logger.LogInformation("No Results found in {leagueName} for ids {ids}", leagueName, ids);
                return NotFound();
            }

            var getResult = await dbResults
                .Select(GetResultModelFromDbExpression)
                .ToListAsync();

            _logger.LogInformation("Return {Count} entries", getResult.Count());

            return Ok(getResult);
        }
    }
}
