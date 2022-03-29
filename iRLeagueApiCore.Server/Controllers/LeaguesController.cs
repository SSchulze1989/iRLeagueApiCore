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
                    LeagueId = x.Id,
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
        public async Task<ActionResult<GetLeagueModel>> Put([FromBody] PutLeagueModel putLeague)
        {
            _logger.LogInformation("Put league data with {LeagueId} by {UserName}", putLeague.LeagueId,
                User.Identity.Name);

            var dbLeague = await _dbContext.FindAsync<LeagueEntity>(putLeague.LeagueId);

            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (dbLeague == null)
            {
                _logger.LogInformation("Creating league {LeagueName} by {UserName}", putLeague.Name,
                    User.Identity.Name);
                // validate leaguename
                if (Regex.IsMatch(putLeague.Name, "^[a-zA-Z0-9_-]*$") == false)
                {
                    _logger.LogInformation("Failed to create league: league {LeagueName} is invalid", putLeague.Name);
                    return BadRequestMessage("Invalid name", "League names may only contain the following characters: a-z A-Z 0-9 _ -");
                }

                // check if league with same name exists
                if (await _dbContext.Leagues
                    .AnyAsync(x => x.Name.ToLower() == putLeague.Name.ToLower()))
                {
                    _logger.LogInformation("Failed to create league: league {LeagueName} already exists", putLeague.Name);
                    return BadRequestMessage("League exists", "A league with the same name exists already and cannot be created");
                }

                dbLeague = new LeagueEntity();
                _dbContext.Leagues.Add(dbLeague);
                dbLeague.CreatedOn = DateTime.Now;
                dbLeague.CreatedByUserId = currentUserID;
                dbLeague.CreatedByUserName = User.Identity.Name;

                _logger.LogInformation("League {leagueName} created successfully");
            }
            dbLeague.LastModifiedOn = DateTime.Now;
            dbLeague.LastModifiedByUserId = currentUserID;
            dbLeague.LastModifiedByUserName = User.Identity.Name;
            dbLeague.Name = putLeague.Name;
            dbLeague.NameFull = putLeague.NameFull;
            await _dbContext.SaveChangesAsync();

            if (dbLeague == null)
            {
                _logger.LogError("Failed to put league data for {LeagueName} due to unknown error", putLeague.Name);
                return SomethingWentWrong();
            }

            var getLeague = new GetLeagueModel();
            getLeague.LeagueId = dbLeague.Id;
            getLeague.Name = dbLeague.Name;
            getLeague.NameFull = dbLeague.NameFull;
            getLeague.CreatedByUserId = dbLeague.CreatedByUserId;
            getLeague.CreatedByUserName = dbLeague.CreatedByUserName;
            getLeague.CreatedOn = dbLeague.CreatedOn;
            getLeague.LastModifiedByUserId = dbLeague.LastModifiedByUserId;
            getLeague.LastModifiedByUserName = dbLeague.LastModifiedByUserName;
            getLeague.LastModifiedOn = dbLeague.LastModifiedOn;

            _logger.LogInformation("Return updated entry for {LeagueName}", putLeague.Name);
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
