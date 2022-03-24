using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        /// <param name="leagueName">[Required] Name of the league</param>
        /// <returns></returns>
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

        /// <summary>
        /// Give a league role to a user
        /// </summary>
        /// <param name="leagueName">[Required] Name of the league</param>
        /// <param name="userRole"><c>RoleName</c> of the role to give to the user named <c>UserName</c></param>
        /// <returns>Action result</returns>
        [HttpPost("GiveRole")]
        public async Task<IActionResult> GiveRole([FromRoute] string leagueName, [FromBody] UserRoleModel userRole)
        {
            _logger.LogInformation("Give league role {LeagueRole} to user {RoleUser} for {LeagueName} by {UserName}",
                userRole.RoleName, userRole.UserName, leagueName, User.Identity.Name);

            // find user in database
            var roleUser = await _userManager.FindByNameAsync(userRole.UserName);
            if (roleUser == null)
            {
                _logger.LogInformation("No user named {RoleUser} found in user database.");
                return BadRequest(new Response() { Status = "User not found", Message = "The username from request body does not exist" });
            }

            // check if league role is valid
            if (LeagueRoles.RolesAvailable.Contains(userRole.RoleName) == false)
            {
                _logger.LogInformation("Role {LeagueRole} is not a valid league role", userRole.RoleName);
                return BadRequest(new Response() { Status = "Role invalid", Message = "The rolename from request body is not a valid league role.\n" +
                    "Possible values are:\n" + string.Join("\n", LeagueRoles.RolesAvailable.Select(x => $"- {x}"))});
            }

            // check if league role exists - if not create it
            // - maybe a bit dirty but this way there is no need to extra create each league role when the league is created first
            //   but only once they are needed
            var leagueRoleName = GetLeagueRoleName(leagueName, userRole.RoleName);
            if (await _roleManager.RoleExistsAsync(leagueRoleName) == false)
            {
                _logger.LogInformation("League role {LeagueRole} does not exist for {LeagueName} and will be created");
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(leagueRoleName));

                if (roleResult.Succeeded == false)
                {
                    _logger.LogError("Failed to create role {Role} due to errors: {Errors}", leagueRoleName, roleResult.Errors);
                    return SomethingWentWrong();
                }

                _logger.LogInformation("League role {LeagueRole} created with full role name {Role}", userRole.RoleName, leagueRoleName);
            }

            var userRoleResult = await _userManager.AddToRoleAsync(roleUser, leagueRoleName);
            if (userRoleResult.Succeeded == false)
            {
                _logger.LogError("Failed to add user {RoleUser} to role {Role} due to errors: {Errors}",
                    userRole.UserName, leagueRoleName, userRoleResult.Errors);
                return SomethingWentWrong();
            }

            _logger.LogInformation("Added league role {LeagueRole} to user {RoleUser} for {LeagueName}",
                userRole.RoleName, userRole.UserName, leagueName);
            return OkMessage($"Role {userRole.RoleName} given to user {userRole.UserName}");
        }

        private static string GetLeagueRoleName(string leagueName, string roleName)
        {
            return $"{leagueName.ToLower()}:{roleName}";
        }
    }
}
