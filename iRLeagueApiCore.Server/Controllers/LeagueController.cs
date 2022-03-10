using iRLeagueApiCore.Communication;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;
using System;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class LeagueController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetLeagueModel>>> Get([FromQuery] long[] ids, [FromServices] LeagueDbContext dbContext)
        {
            IQueryable<LeagueEntity> dbLeagues = dbContext.Leagues;
            var getLeagues = new List<GetLeagueModel>();

            if (ids != null && ids.Count() > 0)
            {
                dbLeagues = dbLeagues.Where(x => ids.Contains(x.LeagueId));
            }

            if (dbLeagues.Count() == 0)
            {
                return NotFound();
            }

            getLeagues = await dbLeagues
                .Select(x => new GetLeagueModel()
                {
                    LeagueId = x.LeagueId,
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
            return Ok(getLeagues);
        }

        [HttpPut]
        public async Task<ActionResult<GetLeagueModel>> Put([FromBody] PutLeagueModel putLeague, [FromServices] LeagueDbContext dbContext)
        {
            var dbLeague = await dbContext.FindAsync<LeagueEntity>(putLeague.LeagueId);
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (dbLeague == null)
            {
                dbLeague = new LeagueEntity();
                dbContext.Leagues.Add(dbLeague);
                dbLeague.CreatedOn = DateTime.Now;
                dbLeague.CreatedByUserId = currentUserID;
                dbLeague.CreatedByUserName = User.Identity.Name;
            }
            dbLeague.LastModifiedOn = DateTime.Now;
            dbLeague.LastModifiedByUserId = currentUserID;
            dbLeague.LastModifiedByUserName = User.Identity.Name;
            dbLeague.Name = putLeague.Name;
            dbLeague.NameFull = putLeague.NameFull;
            dbContext.SaveChanges();

            if (dbLeague == null)
            {
                return BadRequest();
            }

            var getLeague = new GetLeagueModel();
            getLeague.LeagueId = dbLeague.LeagueId;
            getLeague.Name = dbLeague.Name;
            getLeague.NameFull = dbLeague.NameFull;
            getLeague.CreatedByUserId = dbLeague.CreatedByUserId;
            getLeague.CreatedByUserName = dbLeague.CreatedByUserName;
            getLeague.CreatedOn = dbLeague.CreatedOn;
            getLeague.LastModifiedByUserId = dbLeague.LastModifiedByUserId;
            getLeague.LastModifiedByUserName = dbLeague.LastModifiedByUserName;
            getLeague.LastModifiedOn = dbLeague.LastModifiedOn;
            return Ok(getLeague);
        }
    }
}
