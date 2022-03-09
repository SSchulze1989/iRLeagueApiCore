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
    [Route("api/[controller]")]
    [Authorize]
    public class SeasonController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonModel>>> Get([FromQuery] long[] ids, [FromServices] IDbContextFactory<LeagueDbContext> dbContextFactory)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                IQueryable<SeasonEntity> dbSeasons = dbContext.Seasons;

                if (ids != null && ids.Count() > 0)
                {
                    dbSeasons = dbSeasons.Where(x => ids.Contains(x.SeasonId));
                }

                if (dbSeasons.Count() == 0)
                {
                    return NotFound();
                }

                var getSeason = dbContext.Seasons
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
                .ToList();

                return Ok(getSeason);
            }
        }

        [HttpPut]
        public async Task<ActionResult<GetSeasonModel>> Put([FromBody] PutSeasonModel putSeason, [FromServices] IDbContextFactory<LeagueDbContext> dbContextFactory)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var dbSeason = await dbContext.Seasons
                    .Include(x => x.League)
                    .SingleOrDefaultAsync(x => x.SeasonId == putSeason.SeasonId);
                ClaimsPrincipal currentUser = this.User;
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (dbSeason == null)
                {
                    dbSeason = new SeasonEntity()
                    {
                        CreatedOn = DateTime.Now,
                        CreatedByUserId = currentUserID,
                        CreatedByUserName = User.Identity.Name,
                    };
                    dbContext.Seasons.Add(dbSeason);
                }

                if (dbSeason.LeagueId != putSeason.LeagueId)
                {
                    var league = await dbContext.Leagues
                        .Include(x => x.Seasons)
                        .SingleOrDefaultAsync(x => x.LeagueId == putSeason.LeagueId);

                    if (league == null)
                    {
                        return BadRequest($"No league with id {putSeason.LeagueId} found");
                    }

                    if (dbSeason.League != null)
                    {
                        dbSeason.League.Seasons.Remove(dbSeason);
                    }

                    if (league.Seasons.Any(x => x.SeasonId == dbSeason.SeasonId) == false)
                    {
                        league.Seasons.Add(dbSeason);
                    }
                }

                dbSeason.SeasonName = putSeason.SeasonName;
                dbSeason.MainScoring = await dbContext.FindAsync<ScoringEntity>(putSeason.MainScoringId);
                dbSeason.Finished = putSeason.Finished;
                dbSeason.HideCommentsBeforeVoted = putSeason.HideComments;

                await dbContext.SaveChangesAsync();

                var getSeason = dbContext.Seasons
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
                    .FirstOrDefault();

                return Ok(getSeason);
            }
        }
    }
}
