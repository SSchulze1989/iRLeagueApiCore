using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{leagueName}/[controller]")]
    public class SeasonController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonModel>>> Get([FromRoute] string leagueName, [FromQuery] long[] ids, [FromServices] LeagueDbContext dbContext)
        {
            var leagueId = (await dbContext.Leagues
                .Select(x => new {x.LeagueId, x.Name})
                .SingleOrDefaultAsync(x => x.Name == leagueName))
                ?.LeagueId ?? 0;

            IQueryable<SeasonEntity> dbSeasons = dbContext.Seasons
                .Where(x => x.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbSeasons = dbSeasons.Where(x => ids.Contains(x.SeasonId));
            }

            if (dbSeasons.Count() == 0)
            {
                return NotFound();
            }

            var getSeason = await dbSeasons
                .Select(x => new GetSeasonModel()
                {
                    SeasonId = x.SeasonId,
                    SeasonStart = x.SeasonStart,
                    SeasonEnd = x.SeasonEnd,
                    ScheduleIds = x.Schedules.Select(y => y.ScheduleId),
                    SeasonName = x.SeasonName,
                    MainScoringId = x.MainScoringScoringId,
                    Finished = x.Finished,
                    HideComments = x.HideCommentsBeforeVoted,
                    LeagueId = x.LeagueId,
                    CreatedOn = x.CreatedOn,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName
                })
                .ToListAsync();

            return Ok(getSeason);
        }

        [HttpPut]
        public async Task<ActionResult<GetSeasonModel>> Put([FromRoute] string leagueName, [FromBody] PutSeasonModel putSeason, [FromServices] LeagueDbContext dbContext)
        {
            var leagueId = (await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName))
                ?.LeagueId ?? 0;

            var dbSeason = await dbContext.Seasons
                .SingleOrDefaultAsync(x => x.SeasonId == putSeason.SeasonId);
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbSeason == null)
            {
                dbSeason = new SeasonEntity()
                {
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = currentUserID,
                    CreatedByUserName = User.Identity.Name
                };
                dbContext.Seasons.Add(dbSeason);

                var league = dbContext.Leagues
                    .Include(x => x.Seasons)
                    .Single(x => x.LeagueId == leagueId);
                league.Seasons.Add(dbSeason);
            }
            else if (dbSeason.LeagueId != leagueId)
            {
                return Forbid();
            }

            dbSeason.SeasonName = putSeason.SeasonName;
            dbSeason.MainScoring = await dbContext.FindAsync<ScoringEntity>(putSeason.MainScoringId);
            dbSeason.Finished = putSeason.Finished;
            dbSeason.HideCommentsBeforeVoted = putSeason.HideComments;

            await dbContext.SaveChangesAsync();

            var getSeason = await dbContext.Seasons
                .Select(x => new GetSeasonModel()
                {
                    SeasonId = x.SeasonId,
                    SeasonStart = x.SeasonStart,
                    SeasonEnd = x.SeasonEnd,
                    ScheduleIds = x.Schedules.Select(y => y.ScheduleId),
                    SeasonName = x.SeasonName,
                    MainScoringId = x.MainScoringScoringId,
                    Finished = x.Finished,
                    HideComments = x.HideCommentsBeforeVoted,
                    LeagueId = x.LeagueId,
                    CreatedOn = x.CreatedOn,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName
                })
                .Where(x => x.SeasonId == dbSeason.SeasonId)
                .SingleOrDefaultAsync();

            return Ok(getSeason);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromQuery] long id, [FromServices] LeagueDbContext dbContext)
        {
            var leagueId = (await dbContext.Leagues
                .Select(x => new { x.LeagueId, x.Name })
                .SingleOrDefaultAsync(x => x.Name == leagueName))
                ?.LeagueId ?? 0;

            var dbSeason = await dbContext.Seasons
                .SingleOrDefaultAsync(x => x.SeasonId == id);

            if (dbSeason == null)
            {
                return BadRequest($"Season id:{id} does not exist");
            }
            else if (dbSeason.LeagueId != leagueId)
            {
                return Forbid();
            }

            dbContext.Seasons.Remove(dbSeason);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
