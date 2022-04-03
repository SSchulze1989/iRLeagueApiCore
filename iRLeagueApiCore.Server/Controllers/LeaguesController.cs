using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaguesController : LeagueApiController
    {
        private readonly ILogger<LeaguesController> _logger;
        private readonly LeagueDbContext _dbContext;

        public LeaguesController(ILogger<LeaguesController> logger, LeagueDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetLeagueModel>>> Get([FromQuery] long[] ids)
        {
            _logger.LogInformation("Getting league info for league ids {LeagueIds} by {UserName}", ids,
                User.Identity.Name);

            IQueryable<LeagueEntity> dbLeagues = _dbContext.Leagues;
            var getLeagues = new List<GetLeagueModel>();

            if (ids != null && ids.Count() > 0)
            {
                dbLeagues = dbLeagues.Where(x => ids.Contains(x.Id));
            }

            if (dbLeagues.Count() == 0)
            {
                _logger.LogInformation("No leagues found for ids {LeagueIds}", ids);
                return NotFound();
            }

            getLeagues = await dbLeagues
                .Select(x => new GetLeagueModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    NameFull = x.NameFull,
                    SeasonIds = x.Seasons.Select(x => x.SeasonId),
                    CreatedOn = x.CreatedOn,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName
                })
                .ToListAsync();

            _logger.LogInformation("Return {Count} league entries for ids {LeagueIds}", getLeagues.Count(), ids);
            return Ok(getLeagues);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{id}")]
        public async Task<ActionResult<GetLeagueModel>> Put([FromRoute] long id, [FromBody] PutLeagueModel putLeague)
        {
            _logger.LogInformation("Put league data with {LeagueId} by {UserName}", id,
                User.Identity.Name);

            var dbLeague = await _dbContext.FindAsync<LeagueEntity>(id);

            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            dbLeague.LastModifiedOn = DateTime.Now;
            dbLeague.LastModifiedByUserId = currentUserID;
            dbLeague.LastModifiedByUserName = User.Identity.Name;
            dbLeague.NameFull = putLeague.NameFull;
            await _dbContext.SaveChangesAsync();

            var getLeague = new GetLeagueModel();
            getLeague.Id = dbLeague.Id;
            getLeague.Name = dbLeague.Name;
            getLeague.NameFull = dbLeague.NameFull;
            getLeague.CreatedByUserId = dbLeague.CreatedByUserId;
            getLeague.CreatedByUserName = dbLeague.CreatedByUserName;
            getLeague.CreatedOn = dbLeague.CreatedOn;
            getLeague.LastModifiedByUserId = dbLeague.LastModifiedByUserId;
            getLeague.LastModifiedByUserName = dbLeague.LastModifiedByUserName;
            getLeague.LastModifiedOn = dbLeague.LastModifiedOn;

            _logger.LogInformation("Return updated entry for {LeagueName}", getLeague.Name);
            return Ok(getLeague);
        }

        [HttpDelete]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> Delete([FromQuery] long leagueId)
        {
            _logger.LogInformation("Delete league with id {LeagueId} by {UserName}", leagueId,
                User.Identity.Name);
            var dbLeague = await _dbContext.FindAsync<LeagueEntity>(leagueId);

            if (dbLeague == null)
            {
                _logger.LogInformation("Could not delete league: No league with id {LeagueId} found in database", leagueId);
                return NotFound();
            }

            _dbContext.Remove(dbLeague);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted league {LeagueName} with id {LeagueId} by {UserName}", dbLeague.Name, leagueId,
                User.Identity.Name);
            return Ok();
        }
    }
}
