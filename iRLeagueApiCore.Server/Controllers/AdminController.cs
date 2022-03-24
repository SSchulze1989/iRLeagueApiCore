using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(LeagueAuthorizeAttribute))]
    [RequireLeagueRoles(LeagueRoles.Admin)]
    [Route("{leagueName}/[controller]")]
    public class AdminController : LeagueApiController
    {
        private readonly ILogger<AdminController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ILogger<AdminController> logger, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// List the users with at least one role in the current league
        /// </summary>
        [HttpGet("ListUsers")]
        public async Task<ActionResult<IEnumerable<GetAdminUserModel>>> ListUsers([FromRoute] string leagueName)
        {
            _logger.LogInformation("Get list of users for {LeagueName} by {UserName}", leagueName, User.Identity.Name);

            // Get users that have a league role
            var users = new List<ApplicationUser>();

            foreach (var role in LeagueRoles.RolesAvailable)
            {
                users.AddRange(await _userManager.GetUsersInRoleAsync(role));
            }

            if (users.Count() == 0)
            {
                _logger.LogInformation("No users found in {LeagueName}", leagueName);
                return NotFound();
            }

            var getUsers = users
                .Distinct()
                .Select(async x => new GetAdminUserModel()
                {
                    UserName = x.UserName,
                    Firsname = x.FullName.Split(' ').First(),
                    Lastname = x.FullName.Split(' ').Last(),
                    Email = x.Email,
                    Roles = await _userManager.GetRolesAsync(x)
                });

            _logger.LogInformation("Return {Count} user entries for {LeagueName}", getUsers.Count(), leagueName);

            return Ok(getUsers);
        }
    }
}
