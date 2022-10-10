using FluentValidation;
using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Users
{
    public class UsersHandlerBase<THandler, TRequest>
    {
        protected readonly ILogger<THandler> _logger;
        protected readonly UserDbContext userDbContext;
        protected readonly UserManager<ApplicationUser> userManager;
        protected IEnumerable<IValidator<TRequest>> validators;

        public UsersHandlerBase(ILogger<THandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager, IEnumerable<IValidator<TRequest>> validators)
        {
            _logger = logger;
            this.userManager = userManager;
            this.userDbContext = userDbContext;
            this.validators = validators;
        }

        protected async Task<ApplicationUser?> GetUserEntityAsync(string? userId, CancellationToken cancellationToken)
        {
            return await userDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        }

        protected async Task<ApplicationUser?> GetUserAsync(string? userId)
        {
            return await userManager.FindByIdAsync(userId);
        }

        protected UserModel MapToUserModel(ApplicationUser user, UserModel model)
        {
            (var firstname, var lastname) = GetUserFirstnameLastname(user.FullName);
            model.UserId = user.Id;
            model.UserName = user.UserName;
            model.Firstname = firstname;
            model.Lastname = lastname;
            return model;
        }

        protected PrivateUserModel MapToPrivateUserModel(ApplicationUser user, PrivateUserModel model)
        {
            MapToUserModel(user, model);
            model.Email = user.Email;
            return model;
        }

        protected async Task<LeagueUserModel> MapToLeagueUserModelAsync(ApplicationUser user, string leagueName, LeagueUserModel model)
        {
            MapToUserModel(user, model);
            model.LeagueRoles = GetLeagueRoles(leagueName, await userManager.GetRolesAsync(user));
            return model;
        }

        protected async Task<AdminUserModel> MapToAdminUserModelAsync(ApplicationUser user, AdminUserModel model)
        {
            MapToPrivateUserModel(user, model);
            model.Roles = await userManager.GetRolesAsync(user);
            return model;
        }

        protected (string firstname, string lastname) GetUserFirstnameLastname(string? Fullname)
        {
            var parts = Fullname?.Split(';') ?? Array.Empty<string>();
            return (parts.ElementAtOrDefault(0) ?? string.Empty, parts.ElementAtOrDefault(1) ?? string.Empty);
        }

        protected string GetUserFullName(string firstname, string lastname)
        {
            return $"{firstname};{lastname}";
        }

        protected IEnumerable<string> GetLeagueRoles(string leagueName, IEnumerable<string> userRoles)
        {
            return userRoles
                .Where(x => LeagueRoles.IsLeagueRoleName(leagueName, x))
                .Select(x => LeagueRoles.GetRoleName(x)!)
                .ToList();
        }
    }
}
