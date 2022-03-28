using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Admin;
using FluentValidation;
using MediatR;
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
    [RequireLeagueRole(LeagueRoles.Admin)]
    [Route("{leagueName}/[controller]")]
    public class AdminController : LeagueApiController
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IMediator _mediator;


        public AdminController(ILogger<AdminController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// List the users with at least one role in the current league
        /// </summary>
        /// <param name="leagueName">[Required] Name of the league</param>
        /// <returns></returns>
        [HttpGet("ListUsers")]
        public async Task<ActionResult<IEnumerable<GetAdminUserModel>>> ListUsers([FromRoute] string leagueName)
        {
            try
            {
                _logger.LogInformation("Get list of users for {LeagueName} by {UserName}", leagueName, User.Identity.Name);
                var request = new ListUsersRequest(leagueName);
                var getUsers = await _mediator.Send(request);
                if (getUsers.Count() == 0)
                {
                    _logger.LogInformation("No users found in {LeagueName}", leagueName);
                    return NotFound();
                }
                _logger.LogInformation("Return {Count} user entries for {LeagueName}", getUsers.Count(), leagueName);
                return Ok(getUsers);
            }
            catch (ValidationException ex)
            {
                _logger.LogInformation("Bad request - errors: {ValidationErrors}", ex.Errors.Select(x => x.ErrorMessage));
                return ex.ToActionResult();
            }
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
            try
            {
                _logger.LogInformation("Give league role {LeagueRole} to user {RoleUser} for {LeagueName} by {UserName}",
                    userRole.RoleName, userRole.UserName, leagueName, User.Identity.Name);
                var request = new GiveRoleRequest(leagueName, userRole);
                await _mediator.Send(request);
                return OkMessage($"Role {userRole.RoleName} given to user {userRole.UserName}");
            }
            catch (ValidationException ex)
            {
                return ex.ToActionResult();
            }
        }

        //[HttpPost("RevokeRole")]
        //public async Task<IActionResult> RevokeRole([FromRoute] string leagueName, [FromBody] UserRoleModel userRole)
        //{
        //    _logger.LogInformation("Revoke league role {LeagueRole} from user {RoleUser} for {LeagueName} by {UserName}",
        //        userRole.RoleName, userRole.UserName, leagueName, User.Identity.Name);

        //    // find user in database
        //    var roleUser = await _userManager.FindByNameAsync(userRole.UserName);
        //    if (roleUser == null)
        //    {
        //        _logger.LogInformation("No user named {RoleUser} found in user database.");
        //        return BadRequestMessage("User not found", "The username from request body does not exist");
        //    }

        //    // check if league role is valid
        //    if (LeagueRoles.RolesAvailable.Contains(userRole.RoleName) == false)
        //    {
        //        _logger.LogInformation("Role {LeagueRole} is not a valid league role", userRole.RoleName);
        //        return BadRequestMessage("Invalid role",
        //            "The rolename from request body is not a valid league role.\n" +
        //            "Possible values are:\n" + string.Join("\n", LeagueRoles.RolesAvailable.Select(x => $"- {x}")));
        //    }

        //    var leagueRoleName = LeagueRoles.GetLeagueRoleName(leagueName, userRole.RoleName);
        //    // check if user has role
        //    if (await _userManager.IsInRoleAsync(roleUser, leagueRoleName) == false)
        //    {
        //        _logger.LogInformation("User {RoleUser} is not in role {Role}", userRole.UserName, leagueRoleName);
        //        return BadRequestMessage("Not in role", "The role could not be revoked because the user is not in the specified role");
        //    }

        //    var revokeResult = await _userManager.RemoveFromRoleAsync(roleUser, leagueRoleName);
        //    if (revokeResult.Succeeded == false)
        //    {
        //        _logger.LogError("Failed to revoke role {Role} from user {RoleUser} due to errors: {Errors}",
        //            revokeResult.Errors.Select(x => $"{x.Code}: {x.Description}"));
        //        return SomethingWentWrong();
        //    }

        //    _logger.LogInformation("Revoked league role {LeagueRole} from user {RoleUser} for {LeagueName}",
        //        userRole.RoleName, userRole.UserName, leagueName);
        //    return OkMessage($"Role {userRole.RoleName} revoked from user {userRole.UserName}");
        //}
    }
}
