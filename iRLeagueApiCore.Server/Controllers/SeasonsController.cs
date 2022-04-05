using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRole]
    [Route("{leagueName}/[controller]")]
    public class SeasonsController : LeagueApiController
    {
        private readonly ILogger<SeasonsController> _logger;
        private readonly LeagueDbContext _dbContext;

        public SeasonsController(ILogger<SeasonsController> logger, LeagueDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        public async Task<ActionResult<IEnumerable<GetSeasonModel>>> Get([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromQuery] long[] ids)
        {
            _logger.LogInformation("Get seasons from {LeagueName} for ids {SeasonIds} by {UserName}", leagueName, ids,
                User.Identity.Name);

            IQueryable<SeasonEntity> dbSeasons = _dbContext.Seasons
                .Where(x => x.LeagueId == leagueId);

            if (ids != null && ids.Count() > 0)
            {
                dbSeasons = dbSeasons.Where(x => ids.Contains(x.SeasonId));
            }

            if (dbSeasons.Count() == 0)
            {
                _logger.LogInformation("No season found in {LeagueName} for ids {SeasonIds}", leagueName, ids);
                return NotFound();
            }

            var getSeasons = await dbSeasons
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

            _logger.LogInformation("Return {Count} season entries from {LeagueName} for ids {SeasonIds}", getSeasons.Count(),
                leagueName, ids);
            return Ok(getSeasons);
        }

        [HttpPut]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Organizer)]
        [Route("{id}")]
        public async Task<ActionResult<GetSeasonModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long id, [FromBody] PutSeasonModel putSeason)
        {
            _logger.LogInformation("Put season data on {LeagueName} with id {SeasonId} by {UserName}", leagueName,
                id, User.Identity.Name);

            var dbSeason = await _dbContext.Seasons
                .SingleOrDefaultAsync(x => x.SeasonId == id);
            var currentUserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbSeason == null)
            {
                _logger.LogInformation("Create season {SeasonName}", putSeason.SeasonName);
                dbSeason = new SeasonEntity()
                {
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = currentUserID,
                    CreatedByUserName = User.Identity.Name
                };
                _dbContext.Seasons.Add(dbSeason);

                var league = _dbContext.Leagues
                    .Include(x => x.Seasons)
                    .Single(x => x.Id == leagueId);
                league.Seasons.Add(dbSeason);
            }
            else if (dbSeason.LeagueId != leagueId)
            {
                _logger.LogInformation("Season with id {SeasonId} belongs to another league");
                return BadRequestMessage("Season not found", $"No season with id {id} could be found");
            }

            dbSeason.SeasonName = putSeason.SeasonName;
            dbSeason.MainScoring = await _dbContext.Scorings
                .SingleOrDefaultAsync(x => x.ScoringId == putSeason.MainScoringId);
            dbSeason.Finished = putSeason.Finished;
            dbSeason.HideCommentsBeforeVoted = putSeason.HideComments;
            dbSeason.LastModifiedOn = DateTime.Now;
            dbSeason.LastModifiedByUserId = currentUserID;
            dbSeason.LastModifiedByUserName = User.Identity.Name;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Written season data on {LeagueName} for seaon {seasonId} by {UserName}", leagueName,
                dbSeason.SeasonId, User.Identity.Name);

            var getSeason = await _dbContext.Seasons
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

            _logger.LogInformation("Return season entry from {LeagueName} for season {SeasonId}", leagueName,
                getSeason.SeasonId);
            return Ok(getSeason);
        }

        [HttpDelete]
        [ServiceFilter(typeof(InsertLeagueIdAttribute))]
        [RequireLeagueRole(LeagueRoles.Admin)]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromQuery] long id)
        {
            _logger.LogInformation("Deleting season {SeasonId} from {LeagueName} by {UserName}", id, leagueName,
                User.Identity.Name);

            var dbSeason = await _dbContext.Seasons
                .SingleOrDefaultAsync(x => x.SeasonId == id);

            if (dbSeason == null)
            {
                _logger.LogInformation("Not deleted: Season {SeasonId} does not exist", id);
                return NoContent();
            }
            else if (dbSeason.LeagueId != leagueId)
            {
                _logger.LogInformation("Forbid to delete season {SeasonId} because it does not belong to {LeagueName}",
                    id, leagueName);
                return Forbid();
            }

            _dbContext.Seasons.Remove(dbSeason);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted season {SeasonId} from {LeagueName} by {UserName}", id, leagueName,
                User.Identity.Name);
            return Ok();
        }
    }
}
