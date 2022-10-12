﻿using FluentValidation;
using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Users
{
    public record RemoveLeagueRoleRequest(string LeagueName, string UserId, string RoleName) : IRequest<LeagueUserModel>;

    public class RemoveLeagueRoleHandler : UsersHandlerBase<RemoveLeagueRoleHandler, RemoveLeagueRoleRequest>,
        IRequestHandler<RemoveLeagueRoleRequest, LeagueUserModel>
    {
        public RemoveLeagueRoleHandler(ILogger<RemoveLeagueRoleHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager,
            IEnumerable<IValidator<RemoveLeagueRoleRequest>> validators) : base(logger, userDbContext, userManager, validators)
        {
        }

        public async Task<LeagueUserModel> Handle(RemoveLeagueRoleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var user = await GetUserAsync(request.UserId)
                ?? throw new ResourceNotFoundException();
            await RemoveLeagueRoleFromUser(user, request.LeagueName, request.RoleName);
            var getUser = await MapToLeagueUserModelAsync(user, request.LeagueName, new());
            return getUser;
        }

        private async Task RemoveLeagueRoleFromUser(ApplicationUser user, string leagueName, string roleName)
        {
            var leagueRoleName = LeagueRoles.GetLeagueRoleName(leagueName, roleName);
            await userManager.RemoveFromRoleAsync(user, leagueRoleName);
        }
    }
}